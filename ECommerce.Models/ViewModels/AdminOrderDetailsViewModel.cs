namespace ECommerce.Models.ViewModels
{
	public class AdminOrderDetailsViewModel
	{
		public OrderHeader Order { get; set; } = null!;

		public DateTime? ReturnDate { get; set; }
	}
}
