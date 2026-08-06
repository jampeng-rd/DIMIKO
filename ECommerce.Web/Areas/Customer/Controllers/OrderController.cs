using ECommerce.Business.Services.IServices;
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

		public async Task<IActionResult> Index()
		{
			var userId = GetCurrentUserId();

			if (userId == null)
			{
				return Challenge();
			}

			var orders = await _orderService.GetUserOrdersAsync(userId);

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

		private string? GetCurrentUserId()
		{
			return User.FindFirstValue(
				ClaimTypes.NameIdentifier
			);
		}

	}
}
