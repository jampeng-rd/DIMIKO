using ECommerce.Models.Common;

namespace ECommerce.Models.ViewModels
{
	public class AdminDailyOrderListViewModel
	{
		/// <summary>
		/// 目前查詢的台灣日期
		/// </summary>
		public DateTime Date { get; set; }

		/// <summary>
		/// 當日訂單分頁結果
		/// </summary>
		public PagedResult<OrderHeader> PagedOrders { get; set; } = new();

		public int TotalOrders => PagedOrders.TotalCount;

		/// <summary>
		/// 當日所有訂單總金額，不只是目前頁面
		/// </summary>
		public decimal TotalAmount { get; set; }
	}
}
