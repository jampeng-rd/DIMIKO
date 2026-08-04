using ECommerce.Business.Services.IServices;
using ECommerce.Models;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = $"{SD.RoleAdmin},{SD.RoleEmployee}")]
	public class CategoryController : Controller
	{
		private readonly ICategoryService _categoryService;

		public CategoryController(ICategoryService categoryService)
		{
			_categoryService = categoryService;
		}

		public async Task<IActionResult> Index()
		{
			var categories = await _categoryService.GetAllCategoriesAsync();

			return View(categories);
		}

		public async Task<IActionResult> Create()
		{

			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ActionName("Create")]
		public async Task<IActionResult> CreatePOST(Category category)
		{
			// 先檢查 Data Annotations
			if (!ModelState.IsValid)
			{
				return View(category);
			}

			// 基本驗證通過後，再查名稱是否重複
			category.Name = category.Name.Trim();

			bool nameExists = await _categoryService.CategoryNameExistsAsync(category.Name);

			if (nameExists)
			{
				ModelState.AddModelError(nameof(Category.Name), "類別名稱已經存在！");
				return View(category);
			}

			// 所有驗證通過後才寫入資料庫
			await _categoryService.CreateCategoryAsync(category);

			TempData["success"] = "新增成功";

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Update(int? id)
		{
			if(id == null ||  id == 0)
			{
				return NotFound();
			}


			var category = await _categoryService.GetCategoryByIdAsync(id.Value);

			if (category == null)
			{
				return NotFound();
			}

			return View(category);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ActionName("Update")]
		public async Task<IActionResult> UpdatePOST(Category category)
		{
			// 先檢查 Data Annotations
			if (!ModelState.IsValid)
			{
				return View(category);
			}

			// 基本驗證通過後，再查名稱是否重複
			category.Name = category.Name.Trim();

			bool nameExists = await _categoryService.CategoryNameExistsAsync(category.Name, category.Id);

			if (nameExists)
			{
				ModelState.AddModelError(nameof(Category.Name), "類別名稱已經存在！");
				return View(category);
			}

			// 所有驗證通過後才寫入資料庫
			bool updated = await _categoryService.UpdateCategoryAsync(category);

			if (!updated)
			{
				return NotFound();
			}

			TempData["success"] = "更新成功";

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Delete(int? id)
		{
			if (id is null or 0)
			{
				return NotFound();
			}

			var category = await _categoryService.GetCategoryByIdAsync(id.Value);

			if (category == null)
			{
				return NotFound();
			}

			return View(category);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ActionName("Delete")]
		public async Task<IActionResult> DeletePOST(int id)
		{
			bool deleted = await _categoryService.DeleteCategoryAsync(id);

			if (!deleted)
			{
				return NotFound();
			}

			TempData["success"] = "刪除成功";

			return RedirectToAction(nameof(Index));
		}
		
	}
}
