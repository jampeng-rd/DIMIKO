using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
	public class OrderDetail
	{
		public int Id { get; set; }

		public int OrderHeaderId { get; set; }

		[ForeignKey(nameof(OrderHeaderId))]
		[ValidateNever]
		public OrderHeader OrderHeader { get; set; } = null!;

		public int ProductId { get; set; }

		[ForeignKey(nameof(ProductId))]
		[ValidateNever]
		public Product Product { get; set; } = null!;

		[Range(1, 1000, ErrorMessage = "商品數量必須介於 1 到 1000 之間")]
		public int Count { get; set; }

		[Range(typeof(decimal), "0", "99999", ErrorMessage = "商品價格必須介於 0 到 99,999 之間")]
		[Column(TypeName = "decimal(18,2)")]
		public decimal Price { get; set; }
	}
}
