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

		public async Task<IEnumerable<Product>> GetAllProductsAsync(
			bool includeCategory = false, 
			bool includeImages = false)
		{
			IQueryable<Product> query = _dbContext.Products;

			if (includeCategory)
			{
				query = query.Include(product => product.Category);
			}

			if (includeImages)
			{
				query = query.Include(product => product.ProductImages);
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


		public async Task<Product?> GetProductByIdAsync(
			int id,
			bool includeCategory = false,
			bool includeImages = false)
		{
			IQueryable<Product> query = _dbContext.Products;

			if (includeCategory)
			{
				query = query.Include(product => product.Category);
			}

			if (includeImages)
			{
				query = query.Include(product => product.ProductImages);
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

		// 新增照片
		public async Task AddProductImagesAsync(int productId, IEnumerable<ProductImage> productImages)
		{
			bool productExists = await _dbContext.Products.AnyAsync(product => product.Id == productId);

			if (!productExists)
			{
				throw new KeyNotFoundException($"找不到 Id 為 {productId} 的商品。");
			}

			int currentImageCount = await _dbContext.ProductImages.CountAsync(image => image.ProductId == productId);

			bool alreadyHasPrimaryImage = await _dbContext.ProductImages.AnyAsync(
					image => image.ProductId == productId && image.IsPrimary);

			int sortOrder = currentImageCount;

			foreach (var productImage in productImages)
			{
				productImage.ProductId = productId;
				productImage.SortOrder = sortOrder;

				if (!alreadyHasPrimaryImage)
				{
					productImage.IsPrimary = true;
					alreadyHasPrimaryImage = true;
				}
				else
				{
					productImage.IsPrimary = false;
				}

				sortOrder++;
			}

			await _dbContext.ProductImages.AddRangeAsync(productImages);
			await _dbContext.SaveChangesAsync();
		}

		// 取得單張圖片
		public async Task<ProductImage?> GetProductImageByIdAsync(int imageId)
		{
			return await _dbContext.ProductImages
				.AsNoTracking()
				.FirstOrDefaultAsync(image => image.Id == imageId);
		}

		// 刪除圖片資料列
		public async Task<bool> DeleteProductImageAsync(int imageId)
		{
			var image = await _dbContext.ProductImages.FirstOrDefaultAsync(productImage => productImage.Id == imageId);

			if (image == null)
			{
				return false;
			}

			int productId = image.ProductId;
			bool wasPrimary = image.IsPrimary;

			_dbContext.ProductImages.Remove(image);
			await _dbContext.SaveChangesAsync();

			var remainingImages = await _dbContext.ProductImages
				.Where(productImage => productImage.ProductId == productId)
				.OrderBy(productImage => productImage.SortOrder)
				.ToListAsync();

			for (int index = 0; index < remainingImages.Count; index++)
			{
				remainingImages[index].SortOrder = index;
			}

			if (wasPrimary && remainingImages.Count > 0)
			{
				foreach (var remainingImage in remainingImages)
				{
					remainingImage.IsPrimary = false;
				}

				remainingImages[0].IsPrimary = true;
			}

			await _dbContext.SaveChangesAsync();

			return true;
		}

		// 指定首圖
		public async Task<bool> SetPrimaryImageAsync(int productId, int imageId)
		{
			var images = await _dbContext.ProductImages
				.Where(image => image.ProductId == productId)
				.ToListAsync();

			if (images.Count == 0)
			{
				return false;
			}

			var selectedImage = images.FirstOrDefault(image => image.Id == imageId);

			if (selectedImage == null)
			{
				return false;
			}

			foreach (var image in images)
			{
				image.IsPrimary = image.Id == imageId;
			}

			await _dbContext.SaveChangesAsync();

			return true;
		}

	}
}
