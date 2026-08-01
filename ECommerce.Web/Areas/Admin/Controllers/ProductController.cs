using ECommerce.Business.Services.IServices;
using ECommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class ProductController : Controller
	{
		private readonly IProductService _productService;
		private readonly ICategoryService _categoryService;

		public ProductController(
			IProductService productService,
			ICategoryService categoryService)
		{
			_productService = productService;
			_categoryService = categoryService;
		}

		public async Task<IActionResult> Index()
		{
			var products = await _productService.GetAllProductsAsync(includeCategory: true);
			return View(products);
		}

		public async Task<IActionResult> Create()
		{
			await LoadCategoryListAsync();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Product product)
		{
			// 先檢查 Data Annotations
			if (!ModelState.IsValid)
			{
				await LoadCategoryListAsync(product.CategoryId);
				return View(product);
			}

			// 基本驗證通過後，再查 SKU(商品編號) 是否重複
			product.Title = product.Title.Trim();
			product.Description = product.Description.Trim();
			product.SKU = product.SKU.Trim();

			bool skuExists = await _productService.ProductSkuExistsAsync(product.SKU);

			if (skuExists)
			{
				ModelState.AddModelError(nameof(Product.SKU), "商品編號已經存在！");

				await LoadCategoryListAsync(product.CategoryId);
				return View(product);
			}

			// 所有驗證通過後才寫入資料庫
			await _productService.CreateProductAsync(product);

			TempData["success"] = "新增成功";

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Update(int? id)
		{
			if (id == null || id == 0)
			{
				return NotFound();
			}

			var product = await _productService.GetProductByIdAsync(id.Value);

			if (product == null)
			{
				return NotFound();
			}

			await LoadCategoryListAsync(product.CategoryId);
			return View(product);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Update(Product product)
		{
			// 先檢查 Data Annotations
			if (!ModelState.IsValid)
			{
				await LoadCategoryListAsync(product.CategoryId);
				return View(product);
			}

			// 基本驗證通過後，再查 SKU(商品編號) 是否重複
			product.Title = product.Title.Trim();
			product.Description = product.Description.Trim();
			product.SKU = product.SKU.Trim();

			bool skuExists = await _productService.ProductSkuExistsAsync(product.SKU, product.Id);

			if (skuExists)
			{
				ModelState.AddModelError(nameof(Product.SKU), "商品編號已經存在！");

				await LoadCategoryListAsync(product.CategoryId);
				return View(product);
			}

			// 所有驗證通過後才寫入資料庫
			bool updated = await _productService.UpdateProductAsync(product);

			if (!updated)
			{
				return NotFound();
			}

			TempData["success"] = "更新成功";

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || id == 0)
			{
				return NotFound();
			}

			var product = await _productService.GetProductByIdAsync(id.Value);

			if (product == null)
			{
				return NotFound();
			}

			return View(product);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ActionName("Delete")]
		public async Task<IActionResult> DeletePOST(int id)
		{
			bool deleted = await _productService.DeleteProductAsync(id);

			if (!deleted)
			{
				return NotFound();
			}

			TempData["success"] = "刪除成功";

			return RedirectToAction(nameof(Index));
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

	}
}
