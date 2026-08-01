using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
	public class Product
	{
		[Key]
		public int Id { get; set; }

		[Required(ErrorMessage = "請輸入商品名稱")]
		[StringLength(200, ErrorMessage = "商品名稱不可超過 200 個字元")]
		[Display(Name = "商品名稱")]
		public string Title { get; set; } = string.Empty;

		[StringLength(2000, ErrorMessage = "商品描述不可超過 2000 個字元")]
		[Display(Name = "商品描述")]
		public string Description { get; set; } = string.Empty;

		[Required(ErrorMessage = "請輸入商品編號")]
		[StringLength(50, ErrorMessage = "商品編號不可超過 50 個字元")]
		[Display(Name = "商品編號")]
		public string SKU { get; set; } = string.Empty;

		[Required(ErrorMessage = "請輸入原價")]
		[Display(Name = "原價")]
		[Range(typeof(decimal), "0", "99999", ErrorMessage = "原價必須介於 0 到 99,999 之間")]
		[Column(TypeName = "decimal(18,2)")]
		public decimal ListPrice { get; set; }

		[Required(ErrorMessage = "請輸入售價")]
		[Display(Name = "售價")]
		[Range(typeof(decimal), "0", "99999", ErrorMessage = "售價必須介於 0 到 99,999 之間")]
		[Column(TypeName = "decimal(18,2)")]
		public decimal Price { get; set; }

		[Display(Name = "5 件以上價格")]
		[Range(typeof(decimal), "0", "99999", ErrorMessage = "5 件以上價格必須介於 0 到 99,999 之間")]
		[Column(TypeName = "decimal(18,2)")]
		public decimal? Price5 { get; set; }

		[Display(Name = "10 件以上價格")]
		[Range(typeof(decimal), "0", "99999", ErrorMessage = "10 件以上價格必須介於 0 到 99,999 之間")]
		[Column(TypeName = "decimal(18,2)")]
		public decimal? Price10 { get; set; }

		[Required(ErrorMessage = "請輸入庫存數量")]
		[Display(Name = "庫存數量")]
		[Range(0, 9999, ErrorMessage = "庫存數量必須介於 0 到 9,999 之間")]
		public int StockQuantity { get; set; }

		[Display(Name = "商品分類")]
		[Range(1, int.MaxValue, ErrorMessage = "請選擇商品分類")]
		public int CategoryId { get; set; }

		[ValidateNever]
		[ForeignKey(nameof(CategoryId))]
		public Category Category { get; set; } = null!;

		[ValidateNever]
		public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

		[Display(Name = "是否上架")]
		public bool IsActive { get; set; } = true;

	}
}
