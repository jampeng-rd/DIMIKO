namespace ECommerce.Models.ViewModels
{
	public class AdminOrderCalendarViewModel
	{
		/// <summary>
		/// 目前正在瀏覽的年份
		/// </summary>
		public int Year { get; set; }

		/// <summary>
		/// 目前正在瀏覽的月份
		/// </summary>
		public int Month { get; set; }

		/// <summary>
		/// 這個月的第一天
		/// </summary>
		public DateTime MonthStart { get; set; }

		/// <summary>
		/// 這個月共有幾天
		/// </summary>
		public int DaysInMonth { get; set; }

		/// <summary>
		/// 月曆第一天前方需要補幾格
		/// Sunday = 0、Monday = 1...
		/// </summary>
		public int StartDayOffset { get; set; }

		/// <summary>
		/// 該月份的訂單總數
		/// </summary>
		public int TotalOrders { get; set; }

		/// <summary>
		/// Key 為日期，例如 6 代表當月 6 日，
		/// Value 為當日訂單數量
		/// </summary>
		public IReadOnlyDictionary<int, int> DailyOrderCounts { get; set; } = new Dictionary<int, int>();

		public int PreviousYear { get; set; }

		public int PreviousMonth { get; set; }

		public int NextYear { get; set; }

		public int NextMonth { get; set; }
	}
}
