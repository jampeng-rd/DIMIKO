using ECommerce.Business.Services.IServices;
using ECommerce.Utility;
using ECommerce.Utility.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;


namespace ECommerce.Web.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class PaymentController : Controller
	{
		private readonly INewebPayService _newebPayService;
		private readonly IOrderService _orderService;

		public PaymentController(
			INewebPayService newebPayService,
			IOrderService orderService)
		{
			_newebPayService = newebPayService;
			_orderService = orderService;
		}

		// 使用者付款完成後，由瀏覽器回到網站
		[HttpPost]
		[AllowAnonymous]
		[IgnoreAntiforgeryToken]
		public async Task<IActionResult> Return(string TradeInfo, string TradeSha)
		{
			if (string.IsNullOrWhiteSpace(TradeInfo) || string.IsNullOrWhiteSpace(TradeSha))
			{
				TempData["error"] = "付款結果資料不完整";

				return RedirectToAction("Index", "Order", new { area = "Customer" });
			}

			try
			{
				var response = _newebPayService.ValidateAndDecryptPaymentResponse(TradeInfo, TradeSha);

				if (response.Result == null)
				{
					TempData["error"] = "無法取得付款結果";

					return RedirectToAction("Index", "Order", new { area = "Customer" });
				}

				// 依藍新回傳的訂單編號取得訂單
				var order = await _orderService.GetOrderByNumberAsync(response.Result.MerchantOrderNo);

				if (order == null)
				{
					TempData["error"] = "找不到付款對應的訂單";

					return RedirectToAction("Index", "Home", new { area = "Customer" });
				}

				// 付款未成功
				if (!string.Equals(response.Status, "SUCCESS", StringComparison.OrdinalIgnoreCase))
				{
					TempData["error"] = string.IsNullOrWhiteSpace(response.Message)
							? "付款未完成，您可以重新進行付款"
							: $"付款未完成：{response.Message}";

					return RedirectToAction("Details", "Order",
							new
							{
								area = "Customer",
								id = order.Id
							});
				}

				// 付款成功
				TempData["success"] = "付款已完成";

				return RedirectToAction("OrderConfirmation", "Cart",
					new
					{
						area = "Customer",
						orderId = order.Id
					});
			}
			catch (InvalidOperationException exception)
			{
				TempData["error"] = $"付款回傳處理失敗：{exception.Message}";

				return RedirectToAction("Index", "Order", new { area = "Customer" });
			}
			catch (CryptographicException)
			{
				TempData["error"] = "付款回傳處理失敗：TradeInfo 解密失敗";

				return RedirectToAction("Index", "Order", new { area = "Customer" });
			}
			catch (JsonException)
			{
				TempData["error"] = "付款回傳處理失敗：藍新回傳 JSON 解析失敗";

				return RedirectToAction("Index", "Order", new { area = "Customer" });
			}
			catch
			{
				//TempData["error"] = "處理付款結果時發生錯誤，請至我訂單確認付款狀態";
				TempData["error"] = "付款回傳處理失敗：發生其他未預期錯誤";

				return RedirectToAction("Index", "Order", new { area = "Customer" });
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
