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
		/// 目前選擇的訂單狀態
		/// null 代表全部狀態
		/// </summary>
		public string? Status { get; set; }

		/// <summary>
		/// 當日訂單分頁結果
		/// </summary>
		public PagedResult<OrderHeader> PagedOrders { get; set; } = new();

		public int TotalOrders => PagedOrders.TotalCount;

		/// <summary>
		/// 目前篩選條件下的訂單總金額
		/// </summary>
		public decimal TotalAmount { get; set; }
	}
}
