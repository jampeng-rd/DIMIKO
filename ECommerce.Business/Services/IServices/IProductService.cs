using ECommerce.Models;

namespace ECommerce.Business.Services.IServices
{
	public interface IProductService
	{
		Task<IEnumerable<Product>> GetAllProductsAsync(
			bool includeCategory = false,
			bool includeImages = false);

		Task<Product> CreateProductAsync(Product product);

		Task<Product?> GetProductByIdAsync(
			int id,
			bool includeCategory = false,
			bool includeImages = false);

		Task<bool> UpdateProductAsync(Product product);

		Task<bool> DeleteProductAsync(int id);


		Task<bool> ProductSkuExistsAsync(string sku, int? excludedProductId = null);


		Task AddProductImagesAsync(int productId, IEnumerable<ProductImage> productImages);

		Task<ProductImage?> GetProductImageByIdAsync(int imageId);

		Task<bool> DeleteProductImageAsync(int imageId);

		Task<bool> SetPrimaryImageAsync(int productId, int imageId);
	}
}
