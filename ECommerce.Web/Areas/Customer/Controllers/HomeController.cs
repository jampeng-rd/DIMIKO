using ECommerce.Business.Services.IServices;
using ECommerce.Models;
using ECommerce.Models.ViewModels;
using ECommerce.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ECommerce.Web.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class HomeController : Controller
	{
		private readonly IProductService _productService;
		private readonly IShoppingCartService _shoppingCartService;
		private readonly ICategoryService _categoryService;
		private readonly IHeroBannerService _heroBannerService;

		public HomeController(
			IProductService productService,
			IShoppingCartService shoppingCartService,
			ICategoryService categoryService,
			IHeroBannerService heroBannerService)
		{
			_productService = productService;
			_shoppingCartService = shoppingCartService;
			_categoryService = categoryService;
			_heroBannerService = heroBannerService;
		}


		public async Task<IActionResult> Index()
		{
			// 首頁取所有商品顯示
			//var products = await _productService.GetAllProductsAsync(includeCategory: true, includeImages: true);
			//var activeProducts = products.Where(product => product.IsActive).ToList();
			//return View(activeProducts);

			// 首頁取 6 筆商品顯示
			var activeProducts = await _productService.GetLatestActiveProductsAsync(
				6,
				includeCategory: true,
				includeImages: true);

			//return View(activeProducts.ToList());

			// 輪播圖
			var heroBanners = await _heroBannerService.GetActiveHeroBannersAsync();

			var viewModel = new HomeIndexViewModel
			{
				Products = activeProducts.ToList(),
				HeroBanners = heroBanners.ToList()
			};
	
			return View(viewModel);
		}


		// 所有商品頁
		public async Task<IActionResult> Products(
			int? categoryId,
			string? query,
			int page = PaginationSettings.DefaultPageNumber,
			int pageSize = PaginationSettings.DefaultPageSize)
		{
			var categories = (await _categoryService.GetAllCategoriesAsync())
				.OrderBy(category => category.DisplayOrder)
				.ThenBy(category => category.Id)
				.ToList();

			Category? selectedCategory = null;

			if (categoryId.HasValue)
			{
				selectedCategory = categories.FirstOrDefault(category => category.Id == categoryId.Value);

				if (selectedCategory == null)
				{
					return NotFound();
				}
			}

			var products = await _productService.GetPagedActiveProductsAsync(
				pageNumber: page,
				pageSize: pageSize,
				categoryId: categoryId,
				query: query,
				includeCategory: true,
				includeImages: true);

			var viewModel = new ProductListViewModel
			{
				Categories = categories,
				Products = products,
				SelectedCategoryId = categoryId,
				Query = query?.Trim(),
				SelectedCategoryName = selectedCategory?.Name ?? "全部"
			};

			return View(viewModel);
		}


		public async Task<IActionResult> Detail(int? id)
		{
			if (id is null or 0)
			{
				return NotFound();
			}

			var product = await _productService.GetProductByIdAsync(
			   id.Value,
			   includeCategory: true,
			   includeImages: true
		   );

			if (product == null || !product.IsActive)
			{
				return NotFound();
			}

			var viewModel = new ProductDetailViewModel
			{
				Product = product,
				Quantity = 1
			};

			return View(viewModel);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize]
		public async Task<IActionResult> Detail(int productId, int quantity)
		{
			var product = await _productService.GetProductByIdAsync(productId, includeCategory: true, includeImages: true);

			if (product == null || !product.IsActive)
			{
				return NotFound();
			}

			var viewModel = new ProductDetailViewModel
			{
				Product = product,
				Quantity = quantity
			};

			if (quantity < 1 || quantity > 1000)
			{
				ModelState.AddModelError(nameof(ProductDetailViewModel.Quantity), "商品數量必須介於 1 到 1000 之間");
			}

			if (quantity > product.StockQuantity)
			{
				ModelState.AddModelError(nameof(ProductDetailViewModel.Quantity), $"目前庫存僅剩 {product.StockQuantity} 件");
			}

			if (!ModelState.IsValid)
			{
				return View(viewModel);
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrWhiteSpace(userId))
			{
				return Challenge();
			}

			var shoppingCart = new ShoppingCart
			{
				ProductId = product.Id,
				ApplicationUserId = userId,
				Count = quantity
			};

			try
			{
				await _shoppingCartService.AddToCartAsync(shoppingCart);

				var cartCount = await _shoppingCartService.GetCartCountAsync(userId);

				HttpContext.Session.SetInt32(SD.SessionCart, cartCount);

				TempData["success"] = $"已將「{product.Title}」加入購物車";

				return RedirectToAction(nameof(Detail), new { id = product.Id });
			}
			catch (ArgumentException exception)
			{
				ModelState.AddModelError(nameof(ProductDetailViewModel.Quantity), exception.Message);
			}
			catch (InvalidOperationException exception)
			{
				ModelState.AddModelError(nameof(ProductDetailViewModel.Quantity), exception.Message);
			}

			return View(viewModel);
		}

	}
}
