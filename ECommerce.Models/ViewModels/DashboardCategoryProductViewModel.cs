// 各分類商品數量

namespace ECommerce.Models.ViewModels
{
	public class DashboardCategoryProductViewModel
	{
		public string Category { get; set; } = string.Empty;

		public int Count { get; set; }
	}
}
