namespace ECommerce.Models.ViewModels
{
	public class PaginationViewModel
	{
		public string Area { get; set; } = string.Empty;

		public string Controller { get; set; } = string.Empty;

		public string Action { get; set; } = string.Empty;

		public int PageNumber { get; set; }

		public int PageSize { get; set; }

		public int TotalCount { get; set; }

		public int TotalPages { get; set; }

		public int FirstItemNumber { get; set; }

		public int LastItemNumber { get; set; }

		/// <summary>
		/// 分頁時需要保留的額外 QueryString
		/// 例如 date、status、keyword
		/// </summary>
		public Dictionary<string, string?> RouteValues { get; set; } = new();
	}
}
