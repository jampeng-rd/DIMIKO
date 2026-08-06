using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
	public class ShoppingCartViewModel
	{
		public IEnumerable<ShoppingCart> CartItems { get; set; } = new List<ShoppingCart>();

		public OrderHeader OrderHeader { get; set; } = new OrderHeader();

		[Display(Name = "購物車商品數量")]
		public int TotalCount { get; set; }

		[Display(Name = "商品總金額")]
		public decimal OrderTotal { get; set; }
	}
}
