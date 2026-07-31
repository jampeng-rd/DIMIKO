using ECommerce.Business.Services.IServices;
using ECommerce.DataAccess.Data;
using ECommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Business.Services
{
	public class CategoryService : ICategoryService
	{
		private readonly ApplicationDbContext _dbContext;

		public CategoryService(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<Category> CreateCategoryAsync(Category category)
		{
			await _dbContext.Categories.AddAsync(category);
			await _dbContext.SaveChangesAsync();

			return category;
		}

		public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
		{
			return await _dbContext.Categories.ToListAsync();
		}

		public async Task<Category?> GetCategoryByIdAsync(int id)
		{
			return await _dbContext.Categories.FindAsync(id);
			//return await dbContext.Categories.FirstOrDefaultAsync(category => category.Id == id);
		}

		public async Task<bool> UpdateCategoryAsync(Category category)
		{
			var existingCategory = await _dbContext.Categories.FindAsync(category.Id);

			if (existingCategory == null)
			{
				return false;
			}

			existingCategory.Name = category.Name;
			existingCategory.DisplayOrder = category.DisplayOrder;

			await _dbContext.SaveChangesAsync();

			return true;
		}

		public async Task<bool> DeleteCategoryAsync(int id)
		{
			var category = await _dbContext.Categories.FindAsync(id);

			if (category == null)
			{
				return false;
			}

			_dbContext.Categories.Remove(category);
			await _dbContext.SaveChangesAsync();

			return true;
		}


		public async Task<bool> CategoryNameExistsAsync(string name, int? excludedCategoryId = null)
		{
			IQueryable<Category> query = _dbContext.Categories;

			if (excludedCategoryId.HasValue)
			{
				query = query.Where(category => category.Id != excludedCategoryId.Value);
			}

			return await query.AnyAsync(category => category.Name == name);
		}

	}
}
