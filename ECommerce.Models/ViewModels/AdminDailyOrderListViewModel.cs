namespace ECommerce.Models.ViewModels
{
	public class AdminDailyOrderListViewModel
	{
		/// <summary>
		/// 目前查詢的台灣日期
		/// </summary>
		public DateTime Date { get; set; }

		/// <summary>
		/// 該日期的訂單
		/// </summary>
		public IReadOnlyList<OrderHeader> Orders { get; set; } = new List<OrderHeader>();

		public int TotalOrders => Orders.Count;

		public decimal TotalAmount => Orders.Sum(order => order.OrderTotal);
	}
}
