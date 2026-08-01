using Microsoft.AspNetCore.Http;

namespace ECommerce.Business.Services.IServices
{
	public interface IProductImageFileService
	{
		Task<IReadOnlyList<SavedProductImage>> SaveImagesAsync(int productId, IEnumerable<IFormFile> images);

		bool DeleteImage(string imageUrl);

		bool DeleteProductDirectory(int productId);
	}
}
