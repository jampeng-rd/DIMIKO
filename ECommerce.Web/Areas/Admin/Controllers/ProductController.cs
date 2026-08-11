using ECommerce.Business.Services.IServices;
using ECommerce.Models;
using ECommerce.Models.ViewModels;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = $"{SD.RoleAdmin},{SD.RoleEmployee}")]
	public class ProductController : Controller
	{
		private readonly IProductService _productService;
		private readonly ICategoryService _categoryService;
		private readonly IProductImageFileService _productImageFileService;

		public ProductController(
			IProductService productService,
			ICategoryService categoryService,
			IProductImageFileService productImageFileService)
		{
			_productService = productService;
			_categoryService = categoryService;
			_productImageFileService = productImageFileService;
		}

		public async Task<IActionResult> Index(
			int page = PaginationSettings.DefaultPageNumber,
			int pageSize = PaginationSettings.DefaultPageSize)
		{
			var products = await _productService.GetPagedProductsAsync(
				page,
				pageSize,
				includeCategory: true,
				includeImages: true);

			return View(products);
		}


		[Authorize(Roles = SD.RoleAdmin)]
		public async Task<IActionResult> Create()
		{
			await LoadCategoryListAsync();

			return View(new ProductCreateViewModel());
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = SD.RoleAdmin)]
		public async Task<IActionResult> Create(ProductCreateViewModel viewModel)
		{
			Product product = viewModel.Product;

			if (viewModel.Images is null || viewModel.Images.Count == 0)
			{
				ModelState.AddModelError(nameof(ProductCreateViewModel.Images), "請至少選擇一張商品圖片");
			}

			// 檢查 Data Annotations
			if (!ModelState.IsValid)
			{
				await LoadCategoryListAsync(product.CategoryId);
				return View(viewModel);
			}

			// 基本驗證通過後，再查 SKU(商品編號) 是否重複
			product.Title = product.Title.Trim();
			product.Description = product.Description.Trim();
			product.SKU = product.SKU.Trim();

			bool skuExists = await _productService.ProductSkuExistsAsync(product.SKU);

			if (skuExists)
			{
				ModelState.AddModelError("Product.SKU", "商品編號已經存在！");

				await LoadCategoryListAsync(product.CategoryId);

				return View(viewModel);
			}

			// 所有驗證通過後才寫入資料庫
			await _productService.CreateProductAsync(product);

			// 建立 ProductImage 資料列
			try
			{
				//IReadOnlyList<SavedProductImage> savedImages =
				var savedImages = await _productImageFileService.SaveImagesAsync(product.Id, viewModel.Images!);

				List<ProductImage> productImages = savedImages.Select((savedImage, index) =>
							new ProductImage
							{
								ProductId = product.Id,
								FileName = savedImage.FileName,
								ImageUrl = savedImage.ImageUrl,
								SortOrder = index,
								IsPrimary = index == 0
							}).ToList();

				await _productService.AddProductImagesAsync(product.Id, productImages);
			}
			catch (InvalidOperationException exception)
			{
				_productImageFileService.DeleteProductDirectory(product.Id);

				await _productService.DeleteProductAsync(product.Id);

				ModelState.AddModelError(
					nameof(ProductCreateViewModel.Images),
					exception.Message);

				await LoadCategoryListAsync(product.CategoryId);

				return View(viewModel);
			}

			TempData["success"] = "新增成功";

			return RedirectToAction(nameof(Index));
		}


		[Authorize(Roles = SD.RoleAdmin)]
		public async Task<IActionResult> Update(int? id)
		{
			if (id is null or 0)
			{
				return NotFound();
			}

			var product = await _productService.GetProductByIdAsync(id.Value, includeImages: true);

			if (product == null)
			{
				return NotFound();
			}

			await LoadCategoryListAsync(product.CategoryId);

			var viewModel = new ProductUpdateViewModel
			{
				Product = product,
				ExistingImages = product.ProductImages
					.OrderBy(image => image.SortOrder)
					.ToList()
			};

			return View(viewModel);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = SD.RoleAdmin)]
		public async Task<IActionResult> Update(ProductUpdateViewModel viewModel)
		{
			Product product = viewModel.Product;

			// 檢查 Data Annotations
			if (!ModelState.IsValid)
			{
				await PrepareUpdateViewModelAsync(viewModel);

				return View(viewModel);
			}

			// 基本驗證通過後，再查 SKU(商品編號) 是否重複
			product.Title = product.Title.Trim();
			product.Description = product.Description.Trim();
			product.SKU = product.SKU.Trim();

			bool skuExists = await _productService.ProductSkuExistsAsync(product.SKU, product.Id);

			if (skuExists)
			{
				ModelState.AddModelError("Product.SKU", "商品編號已經存在！");

				await PrepareUpdateViewModelAsync(viewModel);

				return View(viewModel);
			}

			// 所有驗證通過後才寫入資料庫
			bool updated = await _productService.UpdateProductAsync(product);

			if (!updated)
			{
				return NotFound();
			}

			if (viewModel.NewImages is { Count: > 0 })
			{
				try
				{
					var savedImages = await _productImageFileService.SaveImagesAsync(product.Id, viewModel.NewImages);

					var productImages = savedImages.Select(savedImage => new ProductImage
						{
							ProductId = product.Id,
							FileName = savedImage.FileName,
							ImageUrl = savedImage.ImageUrl
						}).ToList();

					await _productService.AddProductImagesAsync(product.Id, productImages);
				}
				catch (InvalidOperationException exception)
				{
					ModelState.AddModelError(
						nameof(ProductUpdateViewModel.NewImages),
						exception.Message);

					await PrepareUpdateViewModelAsync(viewModel);

					return View(viewModel);
				}
			}

			TempData["success"] = "更新成功";

			return RedirectToAction(nameof(Index));
		}


		[Authorize(Roles = SD.RoleAdmin)]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || id == 0)
			{
				return NotFound();
			}

			var product = await _productService.GetProductByIdAsync(id.Value, includeImages: true);

			if (product == null)
			{
				return NotFound();
			}

			return View(product);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		[ActionName("Delete")]
		[Authorize(Roles = SD.RoleAdmin)]
		public async Task<IActionResult> DeletePOST(int id)
		{
			bool deleted = await _productService.DeleteProductAsync(id);

			if (!deleted)
			{
				return NotFound();
			}

			// 刪除 wwwroot 中的商品圖片資料夾。 資料夾不存在時回傳 false，不影響商品刪除結果。
			_productImageFileService.DeleteProductDirectory(id);

			TempData["success"] = "刪除成功";

			return RedirectToAction(nameof(Index));
		}


		//刪除圖片
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = SD.RoleAdmin)]
		public async Task<IActionResult> DeleteImage(int imageId, int productId)
		{
			var image = await _productService.GetProductImageByIdAsync(imageId);

			if (image == null || image.ProductId != productId)
			{
				return NotFound();
			}

			bool fileDeleted = _productImageFileService.DeleteImage(image.ImageUrl);

			bool recordDeleted = await _productService.DeleteProductImageAsync(imageId);

			if (!recordDeleted)
			{
				return NotFound();
			}

			TempData["success"] = fileDeleted
				? "圖片刪除成功"
				: "圖片記錄已刪除，但實體檔案不存在";

			return RedirectToAction(nameof(Update), new { id = productId });
		}


		// 設定首圖
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = SD.RoleAdmin)]
		public async Task<IActionResult> SetPrimaryImage(int imageId, int productId)
		{
			bool updated = await _productService.SetPrimaryImageAsync(productId, imageId);

			if (!updated)
			{
				return NotFound();
			}

			TempData["success"] = "首圖設定成功";

			return RedirectToAction(nameof(Update), new { id = productId });
		}


		private async Task LoadCategoryListAsync(int? selectedCategoryId = null)
		{
			var categories = await _categoryService.GetAllCategoriesAsync();

			ViewBag.CategoryList = new SelectList(
				categories,
				nameof(Category.Id),
				nameof(Category.Name),
				selectedCategoryId);
		}

		private async Task PrepareUpdateViewModelAsync(ProductUpdateViewModel viewModel)
		{
			await LoadCategoryListAsync(viewModel.Product.CategoryId);

			var product = await _productService.GetProductByIdAsync(
				viewModel.Product.Id,
				includeImages: true);

			viewModel.ExistingImages = product?.ProductImages
					.OrderBy(image => image.SortOrder)
					.ToList()
					?? [];
		}

	}
}
