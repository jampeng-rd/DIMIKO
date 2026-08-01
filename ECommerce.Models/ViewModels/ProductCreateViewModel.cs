using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
	public class ProductCreateViewModel
	{
		public Product Product { get; set; } = new();

		[Display(Name = "商品圖片")]
		public List<IFormFile>? Images { get; set; }
	}
}
