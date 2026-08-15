using Microsoft.AspNetCore.Http;

namespace ECommerce.Business.Services.IServices
{
	public interface IImageStorageService
	{
		// 上傳單張圖片
		Task<SavedImage> SaveImageAsync(string folder, IFormFile image);

		// 刪除單張圖片
		Task<bool> DeleteImageAsync(string imageUrl);

		// 刪除某個前綴底下全部圖片
		Task<int> DeleteFolderAsync(string folder);
	}
}
