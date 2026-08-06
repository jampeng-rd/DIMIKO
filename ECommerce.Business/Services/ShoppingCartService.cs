using ECommerce.Business.Services.IServices;
using ECommerce.DataAccess.Data;
using ECommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Business.Services
{
	public class ShoppingCartService : IShoppingCartService
	{
		private readonly ApplicationDbContext _dbContext;

		public ShoppingCartService(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}


		public async Task<ShoppingCart?> GetCartByIdAsync(int cartId, string userId)
		{
			return await _dbContext.ShoppingCarts
				.Include(c => c.Product)
				.FirstOrDefaultAsync(c =>
					c.Id == cartId &&
					c.ApplicationUserId == userId);
		}

		public async Task<int> GetCartCountAsync(string userId)
		{
			return await _dbContext.ShoppingCarts
				.Where(u => u.ApplicationUserId == userId)
				.SumAsync(u => u.Count);
		}

		public async Task<IEnumerable<ShoppingCart>> GetUserCartItemsAsync(string userId)
		{
			return await _dbContext.ShoppingCarts
				.AsNoTracking()
				.Include(c => c.Product)
				.ThenInclude(p => p.ProductImages)
				.Where(c => c.ApplicationUserId == userId)
				.ToListAsync();
		}

		public async Task<ShoppingCart> AddToCartAsync(ShoppingCart cart)
		{
			if (string.IsNullOrWhiteSpace(cart.ApplicationUserId))
			{
				throw new ArgumentException("使用者識別碼不可為空白");
			}

			if (cart.Count < 1 || cart.Count > 1000)
			{
				throw new ArgumentOutOfRangeException(nameof(cart.Count), "購物車商品數量必須介於 1 到 1000 之間");
			}

			var product = await _dbContext.Products
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.Id == cart.ProductId && p.IsActive);

			if (product == null)
			{
				throw new InvalidOperationException("商品不存在或目前尚未上架");
			}

			var existingItem = await _dbContext.ShoppingCarts
				.FirstOrDefaultAsync(c =>
					c.ApplicationUserId == cart.ApplicationUserId &&
					c.ProductId == cart.ProductId);

			if (existingItem != null)
			{
				var newCount = existingItem.Count + cart.Count;

				if (newCount > 1000)
				{
					throw new InvalidOperationException("購物車商品數量不可超過 1000 件");
				}

				if (newCount > product.StockQuantity)
				{
					throw new InvalidOperationException("加入的商品數量超過目前庫存");
				}

				existingItem.Count = newCount;

				await _dbContext.SaveChangesAsync();
				return existingItem;
			}

			if (cart.Count > product.StockQuantity)
			{
				throw new InvalidOperationException("加入的商品數量超過目前庫存");
			}


			await _dbContext.ShoppingCarts.AddAsync(cart);
			await _dbContext.SaveChangesAsync();

			return cart;
		}

		// 一般 更新購物車的寫法 (有安全性問題)-直接信任 Controller 傳進來的整個 ShoppingCart
		//public async Task UpdateCartAsync(ShoppingCart cart)
		//{
		//	if (cart.Count <= 0)
		//	{
		//		_dbContext.ShoppingCarts.Remove(cart);
		//	}
		//	else
		//	{
		//		_dbContext.ShoppingCarts.Update(cart);
		//	}
		//	await _dbContext.SaveChangesAsync();
		//}

		// 優化 更新購物車的寫法(資料確實屬於目前登入者、只修改數量、不讓前端改掉 ProductId 或 ApplicationUserId)
		public async Task<bool> UpdateCartQuantityAsync(int cartId, string userId, int count)
		{
			var cart = await _dbContext.ShoppingCarts
				.Include(c => c.Product)
				.FirstOrDefaultAsync(c => c.Id == cartId && c.ApplicationUserId == userId);

			if (cart == null)
			{
				return false;
			}

			if (count <= 0)
			{
				_dbContext.ShoppingCarts.Remove(cart);
				await _dbContext.SaveChangesAsync();
				return true;
			}

			if (count > 1000)
			{
				throw new InvalidOperationException("購物車商品數量不可超過 1000 件");
			}

			if (!cart.Product.IsActive)
			{
				throw new InvalidOperationException("此商品目前已下架");
			}

			if (count > cart.Product.StockQuantity)
			{
				throw new InvalidOperationException("購物車商品數量超過目前庫存");
			}

			cart.Count = count;

			await _dbContext.SaveChangesAsync();
			return true;
		}

		public async Task ClearCartAsync(string userId)
		{
			var cartItems = await _dbContext.ShoppingCarts
				.Where(c => c.ApplicationUserId == userId)
				.ToListAsync();

			if (cartItems.Count == 0)
			{
				return;
			}

			_dbContext.ShoppingCarts.RemoveRange(cartItems);
			await _dbContext.SaveChangesAsync();
		}

		// 移除單一商品
		public async Task<bool> RemoveCartItemAsync(int cartId, string userId)
		{
			var cart = await _dbContext.ShoppingCarts
				.FirstOrDefaultAsync(c => c.Id == cartId && c.ApplicationUserId == userId);

			if (cart == null)
			{
				return false;
			}

			_dbContext.ShoppingCarts.Remove(cart);
			await _dbContext.SaveChangesAsync();

			return true;
		}

	}
}
