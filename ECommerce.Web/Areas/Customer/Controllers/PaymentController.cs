using ECommerce.Business.Services.IServices;
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

				if (!string.Equals(response.Status, "SUCCESS", StringComparison.OrdinalIgnoreCase))
				{
					TempData["error"] = string.IsNullOrWhiteSpace(response.Message)
							? "付款未完成"
							: response.Message;

					return RedirectToAction("Index", "Order", new { area = "Customer" });
				}

				var userId = User.FindFirstValue( ClaimTypes.NameIdentifier);

				if (string.IsNullOrWhiteSpace(userId))
				{
					return RedirectToAction("Index", "Order", new { area = "Customer" });
				}

				var order = await _orderService.GetOrderByNumberAsync(response.Result.MerchantOrderNo, userId);

				if (order == null)
				{
					return NotFound();
				}

				TempData["success"] = "付款完成，訂單付款結果確認中";

				return RedirectToAction("OrderConfirmation", "Cart",
					new
					{
						area = "Customer",
						orderId = order.Id
					});
			}
			catch
			{
				TempData["error"] = "付款結果驗證失敗";

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

	}
}
