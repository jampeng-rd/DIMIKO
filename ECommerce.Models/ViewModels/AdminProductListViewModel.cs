using ECommerce.Models.Common;

namespace ECommerce.Models.ViewModels
{
	public class AdminProductListViewModel
	{
		public PagedResult<Product> Products { get; set; } = new();

		public string? Query { get; set; }
	}
}
