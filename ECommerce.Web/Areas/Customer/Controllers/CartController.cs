using ECommerce.Business.Services.IServices;
using ECommerce.Models.ViewModels;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Web.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class CartController : Controller
	{
		private readonly IShoppingCartService _shoppingCartService;

		public CartController(IShoppingCartService shoppingCartService)
		{
			_shoppingCartService = shoppingCartService;
		}

		public async Task<IActionResult> Index()
		{
			var userId = GetCurrentUserId();

			if (userId == null)
			{
				return Challenge();
			}

			var cartItems = (await _shoppingCartService.GetUserCartItemsAsync(userId)).ToList();

			var viewModel = new ShoppingCartViewModel
			{
				CartItems = cartItems,
				TotalCount = cartItems.Sum(item => item.Count),
				OrderTotal = cartItems.Sum(item => item.Price * item.Count)
			};

			UpdateCartSession(viewModel.TotalCount);

			return View(viewModel);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Increase(int cartId)
		{
			var userId = GetCurrentUserId();

			if (userId == null)
			{
				return Challenge();
			}

			var cartItem = await _shoppingCartService.GetCartByIdAsync(cartId, userId);

			if (cartItem == null)
			{
				return NotFound();
			}

			try
			{
				var updated = await _shoppingCartService.UpdateCartQuantityAsync(cartId, userId, cartItem.Count + 1);

				if (!updated)
				{
					return NotFound();
				}

				await RefreshCartSessionAsync(userId);
			}
			catch (InvalidOperationException exception)
			{
				TempData["error"] = exception.Message;
			}

			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Decrease(int cartId)
		{
			var userId = GetCurrentUserId();

			if (userId == null)
			{
				return Challenge();
			}

			var cartItem = await _shoppingCartService.GetCartByIdAsync(cartId, userId);

			if (cartItem == null)
			{
				return NotFound();
			}

			try
			{
				var updated = await _shoppingCartService.UpdateCartQuantityAsync(cartId, userId, cartItem.Count - 1);

				if (!updated)
				{
					return NotFound();
				}

				await RefreshCartSessionAsync(userId);
			}
			catch (InvalidOperationException exception)
			{
				TempData["error"] = exception.Message;
			}

			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateQuantity(int cartId, int count)
		{
			var userId = GetCurrentUserId();

			if (userId == null)
			{
				return Challenge();
			}

			try
			{
				var updated = await _shoppingCartService.UpdateCartQuantityAsync(cartId, userId, count);

				if (!updated)
				{
					return NotFound();
				}

				await RefreshCartSessionAsync(userId);
			}
			catch (InvalidOperationException exception)
			{
				TempData["error"] = exception.Message;
			}

			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Remove(int cartId)
		{
			var userId = GetCurrentUserId();

			if (userId == null)
			{
				return Challenge();
			}

			var removed = await _shoppingCartService.RemoveCartItemAsync(cartId, userId);

			if (!removed)
			{
				return NotFound();
			}

			await RefreshCartSessionAsync(userId);

			TempData["success"] = "商品已從購物車移除";

			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Clear()
		{
			var userId = GetCurrentUserId();

			if (userId == null)
			{
				return Challenge();
			}

			await _shoppingCartService.ClearCartAsync(userId);

			UpdateCartSession(0);

			TempData["success"] = "購物車已清空";

			return RedirectToAction(nameof(Index));
		}






		private string? GetCurrentUserId()
		{
			return User.FindFirstValue(ClaimTypes.NameIdentifier);
		}

		private async Task RefreshCartSessionAsync(string userId)
		{
			var cartCount = await _shoppingCartService.GetCartCountAsync(userId);

			UpdateCartSession(cartCount);
		}

		private void UpdateCartSession(int cartCount)
		{
			HttpContext.Session.SetInt32(SD.SessionCart, cartCount);
		}

	}
}
