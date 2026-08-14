namespace ECommerce.Models.ViewModels
{
	public class DashboardProductStockViewModel
	{
		public int ProductId { get; set; }

		public string Title { get; set; } = string.Empty;

		public string SKU { get; set; } = string.Empty;

		public int StockQuantity { get; set; }

		public bool IsLowStock => StockQuantity <= 5;
	}
}
