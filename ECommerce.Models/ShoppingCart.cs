using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
	public class ShoppingCart
	{
		public int Id { get; set; }

		public int ProductId { get; set; }

		[ForeignKey("ProductId")]
		[ValidateNever]
		public Product Product { get; set; } = null!;

		[Range(1, 1000, ErrorMessage = "商品數量必須介於 1 到 1000 之間")]
		[Display(Name = "數量")]
		public int Count { get; set; } = 1;

		public string ApplicationUserId { get; set; } = string.Empty;

		[ForeignKey("ApplicationUserId")]
		[ValidateNever]
		public ApplicationUser ApplicationUser { get; set; } = null!;

		[NotMapped]
		public decimal Price
		{
			get
			{
				if (Product == null) return 0m;


				if (Count >= 10)
				{
					// 沒有級距價格時，退回一般售價
					return Product.Price10 ?? Product.Price5 ?? Product.Price;
				}

				if (Count >= 5)
				{
					return Product.Price5 ?? Product.Price;
				}

				return Product.Price;
			}
		}
	}
}
