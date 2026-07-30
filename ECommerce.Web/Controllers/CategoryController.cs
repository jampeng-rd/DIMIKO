using ECommerce.Web.Data;
using ECommerce.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Web.Controllers
{
	public class CategoryController : Controller
	{
		private readonly ApplicationDbContext dbContext;

		public CategoryController(ApplicationDbContext dbContext)
		{
			this.dbContext = dbContext;
		}

		public async Task<IActionResult> Index()
		{
			var categories = await dbContext.Categories.ToListAsync();

			//return View("Index", categories);
			return View(categories);
		}

		public IActionResult Create()
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

			bool nameExists = await dbContext.Categories.AnyAsync(c => c.Name == category.Name);

			if (nameExists)
			{
				ModelState.AddModelError(nameof(Category.Name), "類別名稱已經存在！");
				return View(category);
			}

			// 所有驗證通過後才寫入資料庫
			dbContext.Categories.Add(category);
			await dbContext.SaveChangesAsync();
			TempData["success"] = "新增成功";

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Update(int? id)
		{
			if(id == null ||  id == 0)
			{
				return NotFound();
			}

			// Method_1: 一般 LINQ 查詢，可以使用任意條件
			// var category = await dbContext.Categories.FirstOrDefaultAsync(c => c.Id == id);

			// Method_2: 單純根據主鍵 Id 尋找
			var category = await dbContext.Categories.FindAsync(id.Value);

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

			bool nameExists = await dbContext.Categories.AnyAsync(c => c.Name == category.Name && c.Id != category.Id);

			if (nameExists)
			{
				ModelState.AddModelError(nameof(Category.Name), "類別名稱已經存在！");
				return View(category);
			}

			// 所有驗證通過後才寫入資料庫
			dbContext.Categories.Update(category);
			await dbContext.SaveChangesAsync();
			TempData["success"] = "更新成功";

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Delete(int? id)
		{
			if (id is null or 0)
			{
				return NotFound();
			}

			var category = await dbContext.Categories.FindAsync(id.Value);

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
			var category = await dbContext.Categories.FindAsync(id);

			if (category == null)
			{
				return NotFound();
			}

			dbContext.Categories.Remove(category);
			await dbContext.SaveChangesAsync();
			TempData["success"] = "刪除成功";

			return RedirectToAction(nameof(Index));
		}

	}
}
