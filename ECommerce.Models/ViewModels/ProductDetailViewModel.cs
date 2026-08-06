
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
	public class ProductDetailViewModel
	{
		public Product Product { get; set; } = null!;

		[Range(1, 1000, ErrorMessage = "商品數量必須介於 1 到 1000 之間")]
		public int Quantity { get; set; } = 1;
	}
}
