// 前台商品頁顯示用
using ECommerce.Models.Common;

namespace ECommerce.Models.ViewModels
{
	public class ProductListViewModel
	{
		// 左側分類
		public IReadOnlyList<Category> Categories { get; set; } = new List<Category>();

		// 右側商品分頁
		public PagedResult<Product> Products { get; set; } = new();

		// 現在選哪一類
		public int? SelectedCategoryId { get; set; }

		// 右側標題
		public string SelectedCategoryName { get; set; } = "全部商品";
	}
}
