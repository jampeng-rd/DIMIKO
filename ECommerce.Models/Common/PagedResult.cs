namespace ECommerce.Models.Common
{
	public class PagedResult<T>
	{
		public IReadOnlyList<T> Items { get; set; } = new List<T>();

		public int PageNumber { get; set; }

		public int PageSize { get; set; }

		public int TotalCount { get; set; }

		public int TotalPages => TotalCount == 0
				? 0
				: (int)Math.Ceiling(TotalCount / (double)PageSize);

		public bool HasPreviousPage => PageNumber > 1;

		public bool HasNextPage => PageNumber < TotalPages;

		public int FirstItemNumber => TotalCount == 0
				? 0
				: (PageNumber - 1) * PageSize + 1;

		public int LastItemNumber => Math.Min(PageNumber * PageSize, TotalCount);
	}
}
