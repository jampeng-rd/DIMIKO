using ECommerce.Models;
using ECommerce.Models.Common;

namespace ECommerce.Business.Services.IServices
{
	public interface IProductService
	{
		// 前台 : 首頁取 n 筆商品
		Task<IReadOnlyList<Product>> GetLatestActiveProductsAsync(int count, bool includeCategory = false, bool includeImages = false);

		// 前台 : 產品頁
		Task<PagedResult<Product>> GetPagedActiveProductsAsync(
			int pageNumber,
			int pageSize,
			int? categoryId = null,
			bool includeCategory = false,
			bool includeImages = false);


		Task<IEnumerable<Product>> GetAllProductsAsync(bool includeCategory = false, bool includeImages = false);

		// 後台 : 根據分頁取所有商品
		Task<PagedResult<Product>> GetPagedProductsAsync(
			int pageNumber,
			int pageSize,
			bool includeCategory = false,
			bool includeImages = false);

		// 後台 : 
		Task<Product> CreateProductAsync(Product product);

		// 後台 : 
		Task<Product?> GetProductByIdAsync(
			int id,
			bool includeCategory = false,
			bool includeImages = false);

		// 後台 : 
		Task<bool> UpdateProductAsync(Product product);

		// 後台 : 
		Task<bool> DeleteProductAsync(int id);


		Task<bool> ProductSkuExistsAsync(string sku, int? excludedProductId = null);


		Task AddProductImagesAsync(int productId, IEnumerable<ProductImage> productImages);

		Task<ProductImage?> GetProductImageByIdAsync(int imageId);

		Task<bool> DeleteProductImageAsync(int imageId);

		Task<bool> SetPrimaryImageAsync(int productId, int imageId);

	}
}
