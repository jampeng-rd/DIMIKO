using ECommerce.Business.Services.IServices;
using ECommerce.DataAccess.Data;
using ECommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Business.Services
{
	public class ProductService : IProductService
	{
		private readonly ApplicationDbContext _dbContext;

		public ProductService(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<IEnumerable<Product>> GetAllProductsAsync(bool includeCategory = false)
		{
			IQueryable<Product> query = _dbContext.Products;

			if (includeCategory)
			{
				query = query.Include(product => product.Category);
			}

			return await query
				.AsNoTracking()
				.ToListAsync();
		}

		public async Task<Product> CreateProductAsync(Product product)
		{
			await _dbContext.Products.AddAsync(product);
			await _dbContext.SaveChangesAsync();

			return product;
		}


		public async Task<Product?> GetProductByIdAsync(int id, bool includeCategory = false)
		{
			IQueryable<Product> query = _dbContext.Products;

			if (includeCategory)
			{
				query = query.Include(product => product.Category);
			}

			return await query
				.AsNoTracking()
				.FirstOrDefaultAsync(product => product.Id == id);
		}

		public async Task<bool> UpdateProductAsync(Product product)
		{
			var existingProduct = await _dbContext.Products.FindAsync(product.Id);

			if (existingProduct == null)
			{
				return false;
			}

			existingProduct.Title = product.Title;
			existingProduct.Description = product.Description;
			existingProduct.SKU = product.SKU;
			existingProduct.ListPrice = product.ListPrice;
			existingProduct.Price = product.Price;
			existingProduct.Price5 = product.Price5;
			existingProduct.Price10 = product.Price10;
			existingProduct.StockQuantity = product.StockQuantity;
			existingProduct.CategoryId = product.CategoryId;
			existingProduct.ImageUrl = product.ImageUrl;
			existingProduct.IsActive = product.IsActive;

			await _dbContext.SaveChangesAsync();

			return true;
		}

		public async Task<bool> DeleteProductAsync(int id)
		{
			var product = await _dbContext.Products.FindAsync(id);

			if (product == null)
			{
				return false;
			}

			_dbContext.Products.Remove(product);
			await _dbContext.SaveChangesAsync();

			return true;
		}


		public async Task<bool> ProductSkuExistsAsync(string sku, int? excludedProductId = null)
		{
			IQueryable<Product> query = _dbContext.Products;

			if (excludedProductId.HasValue)
			{
				query = query.Where(product => product.Id != excludedProductId.Value);
			}

			return await query.AnyAsync(product => product.SKU == sku);
		}

	}
}
