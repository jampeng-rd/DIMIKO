using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
	public class ProductImage
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[StringLength(2048)]
		public string ImageUrl { get; set; } = string.Empty;

		[Required]
		[StringLength(255)]
		public string FileName { get; set; } = string.Empty;

		[Display(Name = "顯示順序")]
		public int SortOrder { get; set; }

		[Display(Name = "是否為首圖")]
		public bool IsPrimary { get; set; }

		public int ProductId { get; set; }

		[ValidateNever]
		[ForeignKey(nameof(ProductId))]
		public Product Product { get; set; } = null!;
	}
}
