using ECommerce.Business.Services.IServices;
using ECommerce.Utility;
using ECommerce.Utility.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;


namespace ECommerce.Web.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class PaymentController : Controller
	{
		private readonly INewebPayService _newebPayService;
		private readonly IOrderService _orderService;
		private readonly ILogger<PaymentController> _logger;

		public PaymentController(
			INewebPayService newebPayService,
			IOrderService orderService,
			ILogger<PaymentController> logger)
		{
			_newebPayService = newebPayService;
			_orderService = orderService;
			_logger = logger;
		}

		// 使用者付款完成後，由瀏覽器回到網站
		[HttpPost]
		[AllowAnonymous]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> Return(int orderId)
		{
			if (orderId <= 0)
			{
				TempData["error"] = "無法取得付款訂單資訊";

				return RedirectToAction("Index", "Home",
					new
					{
						area = "Customer"
					});
			}

			try
			{
				var order = await _orderService.GetOrderByIdAsync(orderId);

				if (order == null)
				{
					TempData["error"] = "找不到付款訂單";

					return RedirectToAction("Index", "Home",
						new
						{
							area = "Customer"
						});
				}

				// Notify 已確認付款成功
				if (order.PaymentStatus == SD.PaymentStatusApproved)
				{
					TempData["success"] = "付款成功";

					return RedirectToAction("OrderConfirmation", "Cart",
						new
						{
							area = "Customer",
							orderId
						});
				}

				// Notify 沒有將付款改為 Approved，代表目前仍是未付款狀態。 
				TempData["error"] = "付款未成功，您可以在付款期限內重新付款";

				return RedirectToAction("Details", "Order",
					new
					{
						area = "Customer",
						id = orderId
					});
			}
			catch (Exception exception)
			{
				_logger.LogError(
					exception,
					"藍新 Return 處理失敗。OrderId: {OrderId}",
					orderId);

				TempData["error"] = "取得付款結果時發生錯誤，請至我的訂單確認付款狀態";

				return RedirectToAction("Details", "Order",
					new
					{
						area = "Customer",
						id = orderId
					});
			}
		}


		// 藍新 Server 背景通知
		[HttpPost]
		[AllowAnonymous]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> Notify(string TradeInfo, string TradeSha)
		{
			if (string.IsNullOrWhiteSpace(TradeInfo) || string.IsNullOrWhiteSpace(TradeSha))
			{
				return Content("FAIL");
			}

			try
			{
				var response = _newebPayService.ValidateAndDecryptPaymentResponse(TradeInfo, TradeSha);

				if (response.Result == null)
				{
					return Content("FAIL");
				}

				if (!string.Equals(response.Status, "SUCCESS", StringComparison.OrdinalIgnoreCase))
				{
					return Content("SUCCESS");
				}

				var paymentDate = ParsePaymentDate(response.Result.PayTime);

				if (paymentDate == null)
				{
					return Content("FAIL");
				}

				var updated = await _orderService.MarkPaymentAsApprovedAsync(
							response.Result.MerchantOrderNo,
							response.Result.Amt,
							response.Result.TradeNo,
							response.Result.PaymentType,
							paymentDate.Value);

				return Content(updated ? "SUCCESS" : "FAIL");
			}
			catch
			{
				return Content("FAIL");
			}
		}

		private static DateTime? ParsePaymentDate(string payTime)
		{
			if (string.IsNullOrWhiteSpace(payTime))
			{
				return null;
			}

			if (!DateTime.TryParseExact(
				payTime,
				"yyyy-MM-dd HH:mm:ss",
				CultureInfo.InvariantCulture,
				DateTimeStyles.None,
				out var taiwanPaymentDate))
			{
				return null;
			}

			taiwanPaymentDate =DateTime.SpecifyKind(taiwanPaymentDate, DateTimeKind.Unspecified);

			return TaiwanTimeHelper.ConvertTaiwanToUtc(taiwanPaymentDate);
		}


		// 重新付款
		[HttpPost]
		[Authorize]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Retry(int orderId)
		{
			if (orderId <= 0)
			{
				return NotFound();
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrWhiteSpace(userId))
			{
				return Challenge();
			}

			var order = await _orderService.GetOrderByIdAsync(orderId, userId);

			if (order == null)
			{
				return NotFound();
			}

			if (order.OrderStatus != SD.OrderStatusPending || order.PaymentStatus != SD.PaymentStatusPending)
			{
				TempData["error"] = "此訂單目前無法重新付款";

				return RedirectToAction("Details", "Order",
					new
					{
						area = "Customer",
						id = orderId
					});
			}

			if (order.PaymentExpireDate == null || order.PaymentExpireDate <= DateTime.UtcNow)
			{
				TempData["error"] = "此訂單已超過付款期限，無法重新付款";

				return RedirectToAction("Details", "Order",
					new
					{
						area = "Customer",
						id = orderId
					});
			}

			return RedirectToAction(nameof(Checkout),
				new
				{
					orderId
				});
		}


		[HttpGet]
		[Authorize]
		public async Task<IActionResult> Checkout(int orderId)
		{
			if (orderId <= 0)
			{
				return NotFound();
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrWhiteSpace(userId))
			{
				return Challenge();
			}

			if (!_newebPayService.IsConfigured())
			{
				TempData["error"] = "金流尚未設定完成，目前無法進行付款";

				return RedirectToAction("Details", "Order",
					new
					{
						area = "Customer",
						id = orderId
					});
			}

			var order = await _orderService.GetOrderByIdAsync(orderId, userId);

			if (order == null)
			{
				return NotFound();
			}

			// 只有待付款訂單才能進入付款頁
			if (order.OrderStatus != SD.OrderStatusPending || order.PaymentStatus != SD.PaymentStatusPending)
			{
				TempData["error"] = "此訂單目前無法進行付款";

				return RedirectToAction("Details", "Order",
					new
					{
						area = "Customer",
						id = orderId
					});
			}

			// 已超過付款期限
			if (order.PaymentExpireDate == null || order.PaymentExpireDate <= DateTime.UtcNow)
			{
				TempData["error"] = "此訂單已超過付款期限，無法進行付款";

				return RedirectToAction("Details", "Order",
					new
					{
						area = "Customer",
						id = orderId
					});
			}

			try
			{
				var paymentRequest = _newebPayService.CreatePaymentRequest(order);

				return View(
					"~/Areas/Customer/Views/Cart/Payment.cshtml",
					paymentRequest);
			}
			catch
			{
				TempData["error"] = "建立付款資料時發生錯誤，請稍後再試";

				return RedirectToAction("Details", "Order",
					new
					{
						area = "Customer",
						id = orderId
					});
			}
		}


	}
}
