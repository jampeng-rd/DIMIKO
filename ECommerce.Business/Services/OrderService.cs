using ECommerce.Business.Services.IServices;
using ECommerce.DataAccess.Data;
using ECommerce.Models;
using ECommerce.Models.Common;
using ECommerce.Models.ServiceResults;
using ECommerce.Utility;
using ECommerce.Utility.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Data;

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


				var now = DateTime.UtcNow;

				orderHeader.OrderNumber = OrderNumberGenerator.Generate();
				orderHeader.ApplicationUserId = userId;
				orderHeader.OrderDate = now;

				// 建立訂單後 1 小時內完成付款
				orderHeader.PaymentExpireDate = now.AddHours(1);

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


		// 前台：取訂單編號
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


		// 「金流專用」查詢 - 用途只限定 金流 Return：依訂單 Id 取得訂單 (不要給一般前台使用)
		public async Task<OrderHeader?> GetOrderByIdAsync(int orderId)
		{
			if (orderId <= 0)
			{
				return null;
			}

			return await _dbContext.OrderHeaders
				.AsNoTracking()
				.FirstOrDefaultAsync(order => order.Id == orderId);
		}


		// 前台：建立一次新的付款交易 (每一次付款都會建立一筆交易訂單)
		public async Task<PaymentTransaction?> CreatePaymentTransactionAsync(int orderId, string userId)
		{
			if (orderId <= 0 || string.IsNullOrWhiteSpace(userId))
			{
				return null;
			}

			var order = await _dbContext.OrderHeaders
				.FirstOrDefaultAsync(order =>
					order.Id == orderId &&
					order.ApplicationUserId == userId);

			if (order == null)
			{
				return null;
			}

			// 只有待付款、尚未取消的訂單才能建立付款交易
			if (order.OrderStatus != SD.OrderStatusPending ||
				order.PaymentStatus != SD.PaymentStatusPending)
			{
				return null;
			}

			// 已超過付款期限
			if (order.PaymentExpireDate == null ||
				order.PaymentExpireDate <= DateTime.UtcNow)
			{
				return null;
			}

			var paymentTransaction = new PaymentTransaction
			{
				OrderHeaderId = order.Id,

				MerchantOrderNo = PaymentNumberGenerator.Generate(),

				Amount = order.OrderTotal,

				Status = SD.PaymentTransactionPending,

				CreatedDate = DateTime.UtcNow
			};

			await _dbContext.PaymentTransactions.AddAsync(paymentTransaction);

			await _dbContext.SaveChangesAsync();

			return paymentTransaction;
		}


		// 藍新付款成功：更新付款交易與訂單
		public async Task<bool> MarkPaymentTransactionAsSuccessAsync(
			string merchantOrderNo,
			int amount,
			string tradeNo,
			string paymentType,
			DateTime paymentDate)
		{
			if (string.IsNullOrWhiteSpace(merchantOrderNo))
			{
				return false;
			}

			if (amount <= 0)
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(tradeNo))
			{
				return false;
			}

			await using var transaction = await _dbContext.Database.BeginTransactionAsync();

			try
			{
				var paymentTransaction = await _dbContext.PaymentTransactions
						.Include(payment => payment.OrderHeader)
						.FirstOrDefaultAsync(payment =>
							payment.MerchantOrderNo == merchantOrderNo);

				if (paymentTransaction == null)
				{
					return false;
				}

				var order = paymentTransaction.OrderHeader;

				// 驗證本次付款交易金額
				var transactionAmount = decimal.ToInt32(paymentTransaction.Amount);

				if (transactionAmount != amount)
				{
					return false;
				}

				// 再驗證訂單總金額
				var orderAmount = decimal.ToInt32(order.OrderTotal);

				if (orderAmount != amount)
				{
					return false;
				}

				// 已取消的訂單不能再付款成功
				if (order.OrderStatus == SD.OrderStatusCancelled)
				{
					return false;
				}

				// 實際付款時間已超過付款期限
				if (order.PaymentExpireDate.HasValue &&
					paymentDate > order.PaymentExpireDate.Value)
				{
					return false;
				}

				// Notify 可能重送。
				// 同一筆付款交易已經成功時，不重複更新。
				if (paymentTransaction.Status == SD.PaymentTransactionSuccess)
				{
					var sameTradeNo =
						string.Equals(
							paymentTransaction.NewebPayTradeNo,
							tradeNo,
							StringComparison.Ordinal);

					await transaction.CommitAsync();

					return sameTradeNo;
				}

				// 這張訂單如果已由其他付款交易付款成功，
				// 不允許另一筆交易覆蓋付款資訊。
				if (order.PaymentStatus == SD.PaymentStatusApproved)
				{
					return false;
				}

				// 更新單次付款交易
				paymentTransaction.Status = SD.PaymentTransactionSuccess;

				paymentTransaction.NewebPayTradeNo = tradeNo.Trim();

				paymentTransaction.PaymentType =
					string.IsNullOrWhiteSpace(paymentType)
						? null
						: paymentType.Trim();

				paymentTransaction.PaymentDate = paymentDate;

				paymentTransaction.Message = null;


				// 更新整張訂單的最終付款摘要
				order.PaymentStatus = SD.PaymentStatusApproved;

				order.NewebPayTradeNo = tradeNo.Trim();

				order.PaymentType =
					string.IsNullOrWhiteSpace(paymentType)
						? null
						: paymentType.Trim();

				order.PaymentDate = paymentDate;

				await _dbContext.SaveChangesAsync();

				await transaction.CommitAsync();

				return true;
			}
			catch
			{
				await transaction.RollbackAsync();

				throw;
			}
		}


		// 藍新付款失敗：只更新本次付款交易
		public async Task<bool> MarkPaymentTransactionAsFailedAsync(
			string merchantOrderNo,
			int amount,
			string? message)
		{
			if (string.IsNullOrWhiteSpace(merchantOrderNo))
			{
				return false;
			}

			if (amount <= 0)
			{
				return false;
			}

			var paymentTransaction = await _dbContext.PaymentTransactions
					.Include(payment => payment.OrderHeader)
					.FirstOrDefaultAsync(payment =>
						payment.MerchantOrderNo == merchantOrderNo);

			if (paymentTransaction == null)
			{
				return false;
			}

			var transactionAmount = decimal.ToInt32(paymentTransaction.Amount);

			if (transactionAmount != amount)
			{
				return false;
			}

			var orderAmount = decimal.ToInt32(paymentTransaction.OrderHeader.OrderTotal);

			if (orderAmount != amount)
			{
				return false;
			}

			// 如果同一筆交易已經成功，不可以被後來的重複通知改回 Failed。
			if (paymentTransaction.Status == SD.PaymentTransactionSuccess)
			{
				return true;
			}

			paymentTransaction.Status = SD.PaymentTransactionFailed;

			if (string.IsNullOrWhiteSpace(message))
			{
				paymentTransaction.Message = null;
			}
			else
			{
				var trimmedMessage = message.Trim();

				paymentTransaction.Message =
					trimmedMessage.Length <= 500
						? trimmedMessage
						: trimmedMessage[..500];
			}

			await _dbContext.SaveChangesAsync();

			return true;
		}


		// 金流回傳(藍新 Return / Notify)：依訂單編號取得訂單
		public async Task<OrderHeader?> GetOrderByNumberAsync(string orderNumber)
		{
			if (string.IsNullOrWhiteSpace(orderNumber))
			{
				return null;
			}

			return await _dbContext.OrderHeaders
				.AsNoTracking()
				.FirstOrDefaultAsync(order =>
					order.OrderNumber == orderNumber);
		}


		// 前台：使用者依訂單編號找訂單
		public async Task<OrderHeader?> GetOrderByNumberAsync(string orderNumber, string userId)
		{
			if (string.IsNullOrWhiteSpace(orderNumber) || string.IsNullOrWhiteSpace(userId))
			{
				return null;
			}

			return await _dbContext.OrderHeaders
				.AsNoTracking()
				.FirstOrDefaultAsync(order =>
					order.OrderNumber == orderNumber &&
					order.ApplicationUserId == userId);
		}


		// 前台：取使用者所有訂單資料
		//public async Task<IEnumerable<OrderHeader>> GetUserOrdersAsync(string userId)
		//{
		//	if (string.IsNullOrWhiteSpace(userId))
		//	{
		//		return new List<OrderHeader>();
		//	}

		//	return await _dbContext.OrderHeaders
		//		.AsNoTracking()
		//		.Where(order => order.ApplicationUserId == userId)
		//		.OrderByDescending(order => order.OrderDate)
		//		.ToListAsync();
		//}


		// 前台：取得使用者訂單分頁資料
		public async Task<PagedResult<OrderHeader>> GetUserOrdersAsync(string userId, int pageNumber, int pageSize)
		{
			pageNumber = PaginationSettings.NormalizePageNumber(pageNumber);
			pageSize = PaginationSettings.NormalizePageSize(pageSize);

			if (string.IsNullOrWhiteSpace(userId))
			{
				return new PagedResult<OrderHeader>
				{
					PageNumber = pageNumber,
					PageSize = pageSize
				};
			}

			var query = _dbContext.OrderHeaders
				.AsNoTracking()
				.Where(order => order.ApplicationUserId == userId);

			var totalCount = await query.CountAsync();

			var totalPages = totalCount == 0
				? 0
				: (int)Math.Ceiling(totalCount / (double)pageSize);

			if (totalPages > 0 && pageNumber > totalPages)
			{
				pageNumber = totalPages;
			}

			var items = await query
				.OrderByDescending(order => order.OrderDate)
				.ThenByDescending(order => order.Id)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return new PagedResult<OrderHeader>
			{
				Items = items,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalCount = totalCount
			};
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


		// 前台：使用者取消自己的未付款訂單
		public async Task<bool> CancelUserOrderAsync(int orderId, string userId)
		{
			if (orderId <= 0 || string.IsNullOrWhiteSpace(userId))
			{
				return false;
			}

			await using var transaction = await _dbContext.Database.BeginTransactionAsync();

			try
			{
				var order = await _dbContext.OrderHeaders
					.Include(order => order.OrderDetails)
					.ThenInclude(detail => detail.Product)
					.FirstOrDefaultAsync(order =>
						order.Id == orderId &&
						order.ApplicationUserId == userId);

				if (order == null)
				{
					return false;
				}

				// 前台只能取消「尚未付款、尚未處理」的訂單
				if (order.OrderStatus != SD.OrderStatusPending || order.PaymentStatus != SD.PaymentStatusPending)
				{
					return false;
				}

				// 恢復庫存
				foreach (var detail in order.OrderDetails)
				{
					detail.Product.StockQuantity += detail.Count;
				}

				order.OrderStatus = SD.OrderStatusCancelled;

				await _dbContext.SaveChangesAsync();

				await transaction.CommitAsync();

				return true;
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}


		// 系統：取消逾期未付款訂單並恢復庫存
		public async Task<int> CancelExpiredOrdersAsync()
		{
			var now = DateTime.UtcNow;

			await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

			try
			{
				var expiredOrders =
					await _dbContext.OrderHeaders
						.Include(order => order.OrderDetails)
						.ThenInclude(detail => detail.Product)
						.Where(order =>
							order.OrderStatus == SD.OrderStatusPending &&
							order.PaymentStatus == SD.PaymentStatusPending &&
							order.PaymentExpireDate.HasValue &&
							order.PaymentExpireDate.Value <= now)
						.ToListAsync();

				if (expiredOrders.Count == 0)
				{
					await transaction.CommitAsync();

					return 0;
				}

				foreach (var order in expiredOrders)
				{
					// 恢復這張訂單原本保留的庫存
					foreach (var detail in order.OrderDetails)
					{
						detail.Product.StockQuantity += detail.Count;
					}

					// 訂單已逾期，改為取消
					order.OrderStatus = SD.OrderStatusCancelled;
				}

				await _dbContext.SaveChangesAsync();

				await transaction.CommitAsync();

				return expiredOrders.Count;
			}
			catch
			{
				await transaction.RollbackAsync();

				throw;
			}
		}



		// 後台：取得指定月份每日的訂單數量
		public async Task<IReadOnlyDictionary<int, int>>GetMonthlyOrderCountsAsync(int year, int month)
		{
			if (year is < 2000 or > 2100)
			{
				throw new ArgumentOutOfRangeException(nameof(year),"年份必須介於 2000 到 2100 之間");
			}

			if (month is < 1 or > 12)
			{
				throw new ArgumentOutOfRangeException(nameof(month), "月份必須介於 1 到 12 之間");
			}

			
			// 先建立台灣時間的月份範圍：
			// 例如 2026/08/01 00:00 到 2026/09/01 00:00
			var monthStartTaiwan = new DateTime(
				year,
				month,
				1,
				0,
				0,
				0,
				DateTimeKind.Unspecified
			);

			var nextMonthStartTaiwan = monthStartTaiwan.AddMonths(1);

			// 資料庫保存 UTC，因此查詢前轉成 UTC。
			var monthStartUtc = TaiwanTimeHelper.ConvertTaiwanToUtc(monthStartTaiwan);

			var nextMonthStartUtc = TaiwanTimeHelper.ConvertTaiwanToUtc(nextMonthStartTaiwan);


			// 只從資料庫取回 OrderDate，不載入會員、訂單明細或商品資料。
			var orderDates = await _dbContext.OrderHeaders
				.AsNoTracking()
				.Where(order =>
					order.OrderDate >= monthStartUtc &&
					order.OrderDate < nextMonthStartUtc)
				.Select(order => order.OrderDate)
				.ToListAsync();

			// 將 UTC 訂單時間轉回台灣時間，再按照日期統計。
			var dailyCounts = orderDates
				.Select(TaiwanTimeHelper.ConvertUtcToTaiwan)
				.GroupBy(orderDate => orderDate.Day)
				.ToDictionary(
					group => group.Key,
					group => group.Count()
				);

			return dailyCounts;
		}

		// 後台：取得指定日期的 <分頁資料>
		public async Task<PagedResult<OrderHeader>> GetOrdersByDateAsync(
			DateTime taiwanDate,
			string? status,
			int pageNumber,
			int pageSize)
		{
			pageNumber = PaginationSettings.NormalizePageNumber(pageNumber);
			pageSize = PaginationSettings.NormalizePageSize(pageSize);

			var date = taiwanDate.Date;

			var dayStartTaiwan = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);

			var nextDayStartTaiwan = dayStartTaiwan.AddDays(1);

			var dayStartUtc = TaiwanTimeHelper.ConvertTaiwanToUtc(dayStartTaiwan);
			var nextDayStartUtc = TaiwanTimeHelper.ConvertTaiwanToUtc(nextDayStartTaiwan);

			var query = _dbContext.OrderHeaders
				.AsNoTracking()
				.Where(order =>
					order.OrderDate >= dayStartUtc &&
					order.OrderDate < nextDayStartUtc);

			if (!string.IsNullOrWhiteSpace(status))
			{
				query = query.Where(order => order.OrderStatus == status);
			}

			var totalCount = await query.CountAsync();

			var totalPages = totalCount == 0
					? 0
					: (int)Math.Ceiling(totalCount / (double)pageSize);

			
			// 使用者手動輸入超過總頁數的頁碼時，自動調整到最後一頁。	 	
			if (totalPages > 0 && pageNumber > totalPages)
			{
				pageNumber = totalPages;
			}

			var items = await query
				.Include(order => order.ApplicationUser)
				.OrderByDescending(order => order.OrderDate)
				.ThenByDescending(order => order.Id)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return new PagedResult<OrderHeader>
			{
				Items = items,
				PageNumber = pageNumber,
				PageSize = pageSize,
				TotalCount = totalCount
			};
		}

		// 後台：計算當天全部訂單總額
		public async Task<decimal> GetOrderTotalByDateAsync(DateTime taiwanDate, string? status)
		{
			var date = taiwanDate.Date;

			var dayStartTaiwan = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);

			var nextDayStartTaiwan = dayStartTaiwan.AddDays(1);

			var dayStartUtc = TaiwanTimeHelper.ConvertTaiwanToUtc(dayStartTaiwan);
			var nextDayStartUtc = TaiwanTimeHelper.ConvertTaiwanToUtc(nextDayStartTaiwan);

			var query = _dbContext.OrderHeaders
			   .AsNoTracking()
			   .Where(order =>
				   order.OrderDate >= dayStartUtc &&
				   order.OrderDate < nextDayStartUtc);

			if (!string.IsNullOrWhiteSpace(status))
			{
				query = query.Where(order => order.OrderStatus == status);
			}

			return await query.SumAsync(order => (decimal?)order.OrderTotal) ?? 0m;
		}


		// 後台：取得全部訂單 (未使用)
		public async Task<IEnumerable<OrderHeader>> GetAllOrdersAsync()
		{
			return await _dbContext.OrderHeaders
				.AsNoTracking()
				.Include(order => order.ApplicationUser)
				.OrderByDescending(order => order.OrderDate)
				.ToListAsync();
		}

		// 後台：取得單筆完整訂單
		public async Task<OrderHeader?> GetOrderDetailsByIdAsync(int orderId)
		{
			if (orderId <= 0)
			{
				return null;
			}

			return await _dbContext.OrderHeaders
				.AsNoTracking()
				.Include(order => order.ApplicationUser)
				.Include(order => order.OrderDetails)
					.ThenInclude(detail => detail.Product)
					.ThenInclude(product => product.ProductImages)
				.FirstOrDefaultAsync(order => order.Id == orderId);
		}

		// 後台：狀態處理- 待確認 -> 確認訂單
		public async Task<bool> ConfirmOrderAsync(int orderId)
		{
			if (orderId <= 0)
			{
				return false;
			}

			var order = await _dbContext.OrderHeaders.FirstOrDefaultAsync(order => order.Id == orderId);

			if (order == null)
			{
				return false;
			}

			// 只有「訂單待確認 + 已付款」才能確認訂單
			if (order.OrderStatus != SD.OrderStatusPending || order.PaymentStatus != SD.PaymentStatusApproved)
			{
				return false;
			}

			order.OrderStatus = SD.OrderStatusApproved;

			await _dbContext.SaveChangesAsync();

			return true;
		}

		// 後台：狀態處理- 確認訂單 -> 開始處理
		public async Task<bool> StartProcessingOrderAsync(int orderId)
		{
			if (orderId <= 0)
			{
				return false;
			}

			var order = await _dbContext.OrderHeaders.FirstOrDefaultAsync(order => order.Id == orderId);

			if (order == null)
			{
				return false;
			}

			if (order.OrderStatus != SD.OrderStatusApproved)
			{
				return false;
			}

			order.OrderStatus = SD.OrderStatusInProcess;

			await _dbContext.SaveChangesAsync();

			return true;
		}

		// 後台：狀態處理- 開始處理 -> 已出貨
		public async Task<bool> ShipOrderAsync(int orderId, string carrier, string trackingNumber)
		{
			if (orderId <= 0)
			{
				return false;
			}

			if (string.IsNullOrWhiteSpace(carrier) || string.IsNullOrWhiteSpace(trackingNumber))
			{
				return false;
			}

			var order = await _dbContext.OrderHeaders.FirstOrDefaultAsync(order => order.Id == orderId);

			if (order == null)
			{
				return false;
			}

			if (order.OrderStatus != SD.OrderStatusInProcess)
			{
				return false;
			}

			order.Carrier = carrier.Trim();
			order.TrackingNumber = trackingNumber.Trim();
			order.ShippingDate = DateTime.UtcNow;
			order.OrderStatus = SD.OrderStatusShipped;

			await _dbContext.SaveChangesAsync();

			return true;
		}

		// 後台：取消訂單並恢復庫存
		public async Task<bool> CancelOrderAsync(int orderId)
		{
			if (orderId <= 0)
			{
				return false;
			}

			await using var transaction = await _dbContext.Database.BeginTransactionAsync();

			try
			{
				var order = await _dbContext.OrderHeaders
					.Include(order => order.OrderDetails)
					.ThenInclude(detail => detail.Product)
					.FirstOrDefaultAsync(order => order.Id == orderId);

				if (order == null)
				{
					return false;
				}

				var canCancel =
					order.OrderStatus == SD.OrderStatusPending ||
					order.OrderStatus == SD.OrderStatusApproved ||
					order.OrderStatus == SD.OrderStatusInProcess;

				if (!canCancel)
				{
					return false;
				}

				foreach (var detail in order.OrderDetails)
				{
					detail.Product.StockQuantity += detail.Count;
				}

				order.OrderStatus = SD.OrderStatusCancelled;

				await _dbContext.SaveChangesAsync();

				await transaction.CommitAsync();

				return true;
			}
			catch
			{
				await transaction.RollbackAsync();
				throw;
			}
		}

	}
}
