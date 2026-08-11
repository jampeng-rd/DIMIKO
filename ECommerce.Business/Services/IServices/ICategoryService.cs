using ECommerce.Models;
using ECommerce.Models.Common;

namespace ECommerce.Business.Services.IServices
{
	public interface ICategoryService
	{
		Task<Category> CreateCategoryAsync(Category category);

		Task<IEnumerable<Category>> GetAllCategoriesAsync();

		// 根據分頁取所有類型
		Task<PagedResult<Category>> GetPagedCategoriesAsync(int pageNumber, int pageSize);

		Task<Category?> GetCategoryByIdAsync(int id);

		Task<bool> UpdateCategoryAsync(Category category);

		Task<bool> DeleteCategoryAsync(int id);


		Task<bool> CategoryNameExistsAsync(string name, int? excludedCategoryId = null);
	}
}
