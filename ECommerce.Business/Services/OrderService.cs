using ECommerce.Business.Services.IServices;
using ECommerce.DataAccess.Data;
using ECommerce.Models;
using ECommerce.Models.ServiceResults;
using ECommerce.Utility;
using ECommerce.Utility.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Business.Services
{
	public class OrderService : IOrderService
	{
		private readonly ApplicationDbContext _dbContext;

		public OrderService(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<CreateOrderResult> CreateOrderAsync(OrderHeader orderHeader, string userId)
		{
			if (string.IsNullOrWhiteSpace(userId))
			{
				return CreateOrderResult.Failure("無法取得目前登入使用者");
			}

			await using var transaction = await _dbContext.Database.BeginTransactionAsync();

			try
			{
				// 這裡不使用 AsNoTracking，因為後面需要修改商品庫存並刪除購物車資料。
				var cartItems = await _dbContext.ShoppingCarts
					.Include(cart => cart.Product)
					.Where(cart => cart.ApplicationUserId == userId)
					.ToListAsync();

				if (cartItems.Count == 0)
				{
					return CreateOrderResult.Failure("購物車目前沒有商品");
				}

				decimal orderTotal = 0m;

				foreach (var cartItem in cartItems)
				{
					var product = cartItem.Product;

					if (!product.IsActive)
					{
						return CreateOrderResult.Failure($"商品「{product.Title}」目前已下架");
					}

					if (cartItem.Count < 1)
					{
						return CreateOrderResult.Failure($"商品「{product.Title}」的數量不正確");
					}

					if (cartItem.Count > product.StockQuantity)
					{
						return CreateOrderResult.Failure($"商品「{product.Title}」庫存不足，目前僅剩 {product.StockQuantity} 件");
					}

					orderTotal += cartItem.Price * cartItem.Count;
				}

				orderHeader.OrderNumber = OrderNumberGenerator.Generate();
				orderHeader.ApplicationUserId = userId;
				orderHeader.OrderDate = DateTime.UtcNow;
				orderHeader.OrderTotal = orderTotal;
				orderHeader.OrderStatus = SD.OrderStatusPending;
				orderHeader.PaymentStatus = SD.PaymentStatusPending;

				await _dbContext.OrderHeaders.AddAsync(orderHeader);
				await _dbContext.SaveChangesAsync();

				foreach (var cartItem in cartItems)
				{
					var orderDetail = new OrderDetail
					{
						OrderHeaderId = orderHeader.Id,
						ProductId = cartItem.ProductId,
						Count = cartItem.Count,

						// 將下單當下的單價保存到訂單明細
						Price = cartItem.Price
					};

					await _dbContext.OrderDetails.AddAsync(orderDetail);

					// 扣除庫存
					cartItem.Product.StockQuantity -= cartItem.Count;
				}

				// 訂單建立成功後清除購物車
				_dbContext.ShoppingCarts.RemoveRange(cartItems);

				await _dbContext.SaveChangesAsync();
				await transaction.CommitAsync();

				return CreateOrderResult.Success(orderHeader.Id);
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

		// 取訂單編號
		public async Task<OrderHeader?> GetOrderByIdAsync(int orderId, string userId)
		{
			if (orderId <= 0 || string.IsNullOrWhiteSpace(userId))
			{
				return null;
			}

			return await _dbContext.OrderHeaders
				.AsNoTracking()
				.FirstOrDefaultAsync(order =>
					order.Id == orderId &&
					order.ApplicationUserId == userId);
		}

		// 取使用者訂單
		public async Task<IEnumerable<OrderHeader>> GetUserOrdersAsync(string userId)
		{
			if (string.IsNullOrWhiteSpace(userId))
			{
				return new List<OrderHeader>();
			}

			return await _dbContext.OrderHeaders
				.AsNoTracking()
				.Where(order => order.ApplicationUserId == userId)
				.OrderByDescending(order => order.OrderDate)
				.ToListAsync();
		}

		public async Task<OrderHeader?> GetUserOrderDetailsAsync(int orderId, string userId)
		{
			if (orderId <= 0 || string.IsNullOrWhiteSpace(userId))
			{
				return null;
			}

			return await _dbContext.OrderHeaders
				.AsNoTracking()
				.Include(order => order.OrderDetails)
					.ThenInclude(detail => detail.Product)
					.ThenInclude(product => product.ProductImages)
				.FirstOrDefaultAsync(order => order.Id == orderId && order.ApplicationUserId == userId);
		}


	}
}
