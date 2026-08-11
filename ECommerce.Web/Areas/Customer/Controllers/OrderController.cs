using ECommerce.Business.Services.IServices;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Web.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class OrderController : Controller
	{
		private readonly IOrderService _orderService;

		public OrderController(IOrderService orderService)
		{
			_orderService = orderService;
		}


		public async Task<IActionResult> Index(
			int page = PaginationSettings.DefaultPageNumber,
			int pageSize = PaginationSettings.DefaultPageSize)
		{
			var userId = GetCurrentUserId();

			if (userId == null)
			{
				return Challenge();
			}

			var orders = await _orderService.GetUserOrdersAsync(userId, page, pageSize);

			return View(orders);
		}


		public async Task<IActionResult> Details(int? id)
		{
			if (id is null or <= 0)
			{
				return NotFound();
			}

			var userId = GetCurrentUserId();

			if (userId == null)
			{
				return Challenge();
			}

			var order = await _orderService.GetUserOrderDetailsAsync(id.Value, userId);

			if (order == null)
			{
				return NotFound();
			}

			return View(order);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Cancel(int id)
		{
			if (id <= 0)
			{
				return NotFound();
			}

			var userId = GetCurrentUserId();

			if (userId == null)
			{
				return Challenge();
			}

			try
			{
				var cancelled = await _orderService.CancelUserOrderAsync(id, userId);

				if (!cancelled)
				{
					TempData["error"] = "此訂單目前無法取消";

					return RedirectToAction(nameof(Details), new { id });
				}

				TempData["success"] = "訂單已取消，商品庫存已恢復";

				return RedirectToAction(nameof(Details), new { id });
			}
			catch
			{
				TempData["error"] = "取消訂單時發生錯誤，請稍後再試";

				return RedirectToAction(nameof(Details), new { id });
			}
		}


		private string? GetCurrentUserId()
		{
			return User.FindFirstValue(
				ClaimTypes.NameIdentifier
			);
		}

	}
}
