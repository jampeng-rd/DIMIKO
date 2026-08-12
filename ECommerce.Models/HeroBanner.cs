using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models
{
	public class HeroBanner
	{
		public int Id { get; set; }

		[StringLength(200)]
		[Display(Name = "標題")]
		public string? Title { get; set; }

		[StringLength(500)]
		[Display(Name = "說明")]
		public string? Description { get; set; }

		[Required]
		[StringLength(2048)]
		[Display(Name = "圖片")]
		public string ImageUrl { get; set; } = string.Empty;

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
