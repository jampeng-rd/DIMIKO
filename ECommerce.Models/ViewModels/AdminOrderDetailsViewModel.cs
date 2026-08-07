namespace ECommerce.Models.ViewModels
{
	public class AdminOrderDetailsViewModel
	{
		public OrderHeader Order { get; set; } = null!;

		public DateTime? ReturnDate { get; set; }

		public int ReturnPage { get; set; } = 1;

		public int ReturnPageSize { get; set; } = 7;
	}
}
