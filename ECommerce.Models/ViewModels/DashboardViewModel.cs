namespace ECommerce.Models.ViewModels
{
	public class DashboardViewModel
	{
		public int SelectedYear { get; set; }

		public int SelectedMonth { get; set; }

		public decimal MonthlyRevenue { get; set; }

		public int MonthlyOrders { get; set; }

		public int TotalProducts { get; set; }

		public int TotalUsers { get; set; }

		// 每日營收圖表
		public IReadOnlyList<DashboardDailyRevenueViewModel> DailyRevenue { get; set; }
			= new List<DashboardDailyRevenueViewModel>();

		// 每日訂單數圖表
		public IReadOnlyList<DashboardDailyOrderViewModel> DailyOrders { get; set; }
			= new List<DashboardDailyOrderViewModel>();

		// 訂單狀態圖表
		public IReadOnlyList<DashboardOrderStatusViewModel> OrderStatusBreakdown { get; set; }
			= new List<DashboardOrderStatusViewModel>();

		// 商品分類圖表
		public IReadOnlyList<DashboardCategoryProductViewModel> ProductsPerCategory { get; set; }
			= new List<DashboardCategoryProductViewModel>();

	}
}
