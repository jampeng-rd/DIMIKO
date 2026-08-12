using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
	public class HeroBannerCreateViewModel
	{
		[Required(ErrorMessage = "請選擇輪播圖片")]
		[Display(Name = "輪播圖片")]
		public IFormFile? ImageFile { get; set; }

		[StringLength(200)]
		[Display(Name = "標題")]
		public string? Title { get; set; }

		[StringLength(500)]
		[Display(Name = "說明")]
		public string? Description { get; set; }

		[StringLength(100)]
		[Display(Name = "按鈕文字")]
		public string? ButtonText { get; set; }

		[StringLength(2048)]
		[Display(Name = "連結")]
		public string? LinkUrl { get; set; }

		[Display(Name = "排序")]
		public int DisplayOrder { get; set; }

		[Display(Name = "啟用")]
		public bool IsActive { get; set; } = true;
	}
}
