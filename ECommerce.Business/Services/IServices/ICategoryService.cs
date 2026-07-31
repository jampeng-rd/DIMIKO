using ECommerce.Models;

namespace ECommerce.Business.Services.IServices
{
	public interface ICategoryService
	{
		Task<Category> CreateCategoryAsync(Category category);

		Task<IEnumerable<Category>> GetAllCategoriesAsync();

		Task<Category?> GetCategoryByIdAsync(int id);

		Task<bool> UpdateCategoryAsync(Category category);

		Task<bool> DeleteCategoryAsync(int id);


		Task<bool> CategoryNameExistsAsync(string name, int? excludedCategoryId = null);
	}
}
