using ECommerce.Business.Services.IServices;
using ECommerce.Models;
using ECommerce.Models.ViewModels;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.Web.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class CartController : Controller
	{
		private readonly IShoppingCartService _shoppingCartService;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IOrderService _orderService;
		private readonly INewebPayService _newebPayService;

		public CartController(
			IShoppingCartService shoppingCartService,
			UserManager<ApplicationUser> userManager,
			IOrderService orderService,
			INewebPayService newebPayService)
		{
			_shoppingCartService = shoppingCartService;
			_userManager = userManager;
			_orderService = orderService;
			_newebPayService = newebPayService;
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

		// 訂單清單摘要
		public async Task<IActionResult> Summary()
		{
			var userId = GetCurrentUserId();

			if (userId == null)
			{
				return Challenge();
			}

			var applicationUser = await _userManager.GetUserAsync(User);

			if (applicationUser == null)
			{
				return Challenge();
			}

			var cartItems = (await _shoppingCartService.GetUserCartItemsAsync(userId)).ToList();

			if (cartItems.Count == 0)
			{
				TempData["error"] = "購物車目前沒有商品";

				return RedirectToAction(nameof(Index));
			}

			var viewModel = new ShoppingCartViewModel
			{
				CartItems = cartItems,
				TotalCount = cartItems.Sum(item => item.Count),
				OrderTotal = cartItems.Sum(item => item.Price * item.Count),

				OrderHeader = new OrderHeader
				{
					Name = applicationUser.Name,
					PhoneNumber = applicationUser.PhoneNumber ?? string.Empty,
					City = applicationUser.City ?? string.Empty,
					State = applicationUser.State ?? string.Empty,
					StreetAddress = applicationUser.StreetAddress ?? string.Empty,
					PostalCode = applicationUser.PostalCode ?? string.Empty
				}
			};

			return View(viewModel);
		}

		// 送出訂單摘要 > 建立訂單
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Summary(ShoppingCartViewModel viewModel)
		{
			var userId = GetCurrentUserId();

			if (userId == null)
			{
				return Challenge();
			}

			// 前端只需要提交 OrderHeader 的收件資料。
			// CartItems、TotalCount、OrderTotal 都重新從資料庫取得。
			var cartItems = (await _shoppingCartService.GetUserCartItemsAsync(userId)).ToList();

			if (cartItems.Count == 0)
			{
				TempData["error"] = "購物車目前沒有商品";

				return RedirectToAction(nameof(Index));
			}

			// 重新填入畫面所需資料
			viewModel.CartItems = cartItems;
			viewModel.TotalCount = cartItems.Sum(item => item.Count);
			viewModel.OrderTotal = cartItems.Sum(item => item.Price * item.Count);

			// 這些欄位不由使用者輸入，所以避免它們干擾本次收件資料驗證。
			ModelState.Remove("OrderHeader.OrderNumber");
			ModelState.Remove("OrderHeader.ApplicationUserId");
			ModelState.Remove("OrderHeader.OrderStatus");
			ModelState.Remove("OrderHeader.PaymentStatus");

			if (!ModelState.IsValid)
			{
				return View(viewModel);
			}

			if (!_newebPayService.IsConfigured())
			{
				ModelState.AddModelError(string.Empty, "金流尚未設定完成，目前無法進行付款");

				return View(viewModel);
			}

			// 清除使用者輸入資料前後可能存在的空白
			viewModel.OrderHeader.Name = viewModel.OrderHeader.Name.Trim();
			viewModel.OrderHeader.PhoneNumber = viewModel.OrderHeader.PhoneNumber.Trim();
			viewModel.OrderHeader.City = viewModel.OrderHeader.City.Trim();
			viewModel.OrderHeader.State = viewModel.OrderHeader.State.Trim();
			viewModel.OrderHeader.StreetAddress = viewModel.OrderHeader.StreetAddress.Trim();
			viewModel.OrderHeader.PostalCode = viewModel.OrderHeader.PostalCode.Trim();

			try
			{
				var result = await _orderService.CreateOrderAsync(viewModel.OrderHeader, userId);

				if (!result.Succeeded || result.OrderId == null)
				{
					ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "建立訂單失敗");

					// Service 執行期間商品價格或庫存可能改變，因此重新讀取
					cartItems = (await _shoppingCartService.GetUserCartItemsAsync(userId)).ToList();

					viewModel.CartItems = cartItems;
					viewModel.TotalCount = cartItems.Sum(item => item.Count);
					viewModel.OrderTotal = cartItems.Sum(item => item.Price * item.Count);

					return View(viewModel);
				}

				UpdateCartSession(0);

				// 接藍新金流
				var orderHeader = await _orderService.GetOrderByIdAsync(result.OrderId.Value, userId);

				if (orderHeader == null)
				{
					throw new InvalidOperationException("找不到剛建立的訂單");
				}

				var paymentRequest = _newebPayService.CreatePaymentRequest(orderHeader);

				return View("Payment", paymentRequest);

				// 沒有接藍新金流時的回傳資料
				//return RedirectToAction(nameof(OrderConfirmation), new { orderId = result.OrderId.Value });
			}
			catch
			{
				ModelState.AddModelError(string.Empty, "建立訂單時發生錯誤，請稍後再試");

				return View(viewModel);
			}
		}

		// 建立訂單成功，要加上取回訂單編號
		public async Task<IActionResult> OrderConfirmation(int orderId)
		{
			if (orderId <= 0)
			{
				return NotFound();
			}

			var userId = GetCurrentUserId();

			if (userId == null)
			{
				return Challenge();
			}

			var orderHeader = await _orderService.GetOrderByIdAsync(orderId, userId);

			if (orderHeader == null)
			{
				return NotFound();
			}

			return View(orderHeader);
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
