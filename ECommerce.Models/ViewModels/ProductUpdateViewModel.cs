using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
	public class ProductUpdateViewModel
	{
		public Product Product { get; set; } = new();

		[Display(Name = "新增商品圖片")]
		public List<IFormFile>? NewImages { get; set; }

		[ValidateNever]
		public List<ProductImage> ExistingImages { get; set; } = [];
	}
}
