
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.Models
{
	public class Category
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "請輸入{0}")]
		[StringLength(100, ErrorMessage = "{0}最多只能輸入 {1} 個字元")]
		[Display(Name = "分類名稱")]
		public string Name { get; set; } = string.Empty;

		[Required(ErrorMessage = "請輸入{0}")]
		[Range(0, 100, ErrorMessage = "{0}必須介於 {1} 到 {2} 之間")]
		[Display(Name = "顯示順序")]
		public int? DisplayOrder { get; set; }
	}
}
