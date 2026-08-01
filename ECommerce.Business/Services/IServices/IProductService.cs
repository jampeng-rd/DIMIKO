using ECommerce.Models;

namespace ECommerce.Business.Services.IServices
{
	public interface IProductService
	{
		Task<IEnumerable<Product>> GetAllProductsAsync(bool includeCategory = false);

		Task<Product> CreateProductAsync(Product product);

		Task<Product?> GetProductByIdAsync(int id, bool includeCategory = false);

		Task<bool> UpdateProductAsync(Product product);

		Task<bool> DeleteProductAsync(int id);


		Task<bool> ProductSkuExistsAsync(string sku, int? excludedProductId = null);
	}
}
