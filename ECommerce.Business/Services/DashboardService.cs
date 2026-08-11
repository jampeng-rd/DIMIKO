using ECommerce.Business.Services.IServices;
using ECommerce.DataAccess.Data;
using ECommerce.Models.ViewModels;
using ECommerce.Utility;
using ECommerce.Utility.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Business.Services
{
	public class DashboardService : IDashboardService
	{
		private readonly ApplicationDbContext _dbContext;

		public DashboardService(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<DashboardViewModel> GetDashboardSummaryAsync(int year, int month)
		{
			if (year is < 2000 or > 2100)
			{
				throw new ArgumentOutOfRangeException(nameof(year), "年份必須介於 2000 到 2100 之間");
			}

			if (month is < 1 or > 12)
			{
				throw new ArgumentOutOfRangeException(nameof(month), "月份必須介於 1 到 12 之間");
			}


			// 台灣時間的月份起點
			var monthStartTaiwan = new DateTime(
				year,
				month,
				1,
				0,
				0,
				0,
				DateTimeKind.Unspecified);

			var nextMonthStartTaiwan = monthStartTaiwan.AddMonths(1);


			// DB 使用 UTC，因此先轉成 UTC 查詢範圍
			var monthStartUtc = TaiwanTimeHelper.ConvertTaiwanToUtc(monthStartTaiwan);
			var nextMonthStartUtc = TaiwanTimeHelper.ConvertTaiwanToUtc(nextMonthStartTaiwan);


			// 該月份全部訂單數
			var monthlyOrders = await _dbContext.OrderHeaders
				.AsNoTracking()
				.CountAsync(order =>
					order.OrderDate >= monthStartUtc &&
					order.OrderDate < nextMonthStartUtc);


			// 該月份已付款訂單營收
			var monthlyRevenue = await _dbContext.OrderHeaders
				.AsNoTracking()
				.Where(order =>
					order.OrderDate >= monthStartUtc &&
					order.OrderDate < nextMonthStartUtc &&
					order.PaymentStatus == SD.PaymentStatusApproved)
				.SumAsync(order => (decimal?)order.OrderTotal)
				?? 0m;

			// 把該月訂單時間、金額、訂單狀態取回來
			var monthlyOrderData = await _dbContext.OrderHeaders
				.AsNoTracking()
				.Where(order =>
					order.OrderDate >= monthStartUtc &&
					order.OrderDate < nextMonthStartUtc)
				.Select(order => new
				{
					order.OrderDate,
					order.OrderTotal,
					order.PaymentStatus,
					order.OrderStatus
				})
				.ToListAsync();

			// 算該月份實際有幾天
			var daysInMonth = DateTime.DaysInMonth(year, month);


			// 建立每日營收資料
			var dailyRevenue = Enumerable
				.Range(1, daysInMonth)
				.Select(day =>
				{
					var revenue = monthlyOrderData
						.Where(order =>
						{
							var taiwanDate = TaiwanTimeHelper.ConvertUtcToTaiwan(order.OrderDate);

							return taiwanDate.Day == day && order.PaymentStatus == SD.PaymentStatusApproved;
						})
						.Sum(order => order.OrderTotal);

					return new DashboardDailyRevenueViewModel
					{
						Day = day,
						Revenue = revenue
					};
				})
				.ToList();

			// 建立每日訂單數資料
			var dailyOrders = Enumerable
				.Range(1, daysInMonth)
				.Select(day =>
				{
					var count = monthlyOrderData.Count(order =>
					{
						var taiwanDate = TaiwanTimeHelper.ConvertUtcToTaiwan(order.OrderDate);

						return taiwanDate.Day == day;
					});

					return new DashboardDailyOrderViewModel
					{
						Day = day,
						Count = count
					};
				})
				.ToList();

			// 建立訂單狀態統計
			var orderStatusBreakdown = monthlyOrderData
				.GroupBy(order => order.OrderStatus)
				.Select(group => new DashboardOrderStatusViewModel
				{
					Status = group.Key switch
					{
						SD.OrderStatusPending => "待確認",
						SD.OrderStatusApproved => "已確認",
						SD.OrderStatusInProcess => "處理中",
						SD.OrderStatusShipped => "已出貨",
						SD.OrderStatusCancelled => "已取消",
						_ => group.Key
					},

					Count = group.Count()
				})
				.ToList();

			// 建立商品分類統計
			var productsPerCategory = await _dbContext.Categories
				.AsNoTracking()
				.OrderBy(category => category.DisplayOrder)
				.ThenBy(category => category.Id)
				.Select(category => new DashboardCategoryProductViewModel
				{
					Category = category.Name,
					Count = _dbContext.Products.Count(product => product.CategoryId == category.Id)
				})
				.ToListAsync();


			// 目前商品總數
			var totalProducts = await _dbContext.Products.AsNoTracking().CountAsync();

			// 目前會員總數
			var totalUsers = await _dbContext.ApplicationUsers.AsNoTracking().CountAsync();

			return new DashboardViewModel
			{
				SelectedYear = year,
				SelectedMonth = month,
				MonthlyRevenue = monthlyRevenue,
				MonthlyOrders = monthlyOrders,
				TotalProducts = totalProducts,
				TotalUsers = totalUsers,

				DailyRevenue = dailyRevenue,
				DailyOrders = dailyOrders,
				OrderStatusBreakdown = orderStatusBreakdown,
				ProductsPerCategory = productsPerCategory
			};
		}

	}
}
