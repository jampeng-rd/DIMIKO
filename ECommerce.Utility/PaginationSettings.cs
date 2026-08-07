namespace ECommerce.Utility
{
	public static class PaginationSettings
	{
		public const int DefaultPageNumber = 1;

		public const int DefaultPageSize = 7;

		public const int MaximumPageSize = 20;

		public static readonly int[] AllowedPageSizes =
		{
			5,
			7,
			10,
			15,
			20
		};

		public static int NormalizePageNumber(int pageNumber)
		{
			return pageNumber < 1
				? DefaultPageNumber
				: pageNumber;
		}

		public static int NormalizePageSize(int pageSize)
		{
			return AllowedPageSizes.Contains(pageSize)
				? pageSize
				: DefaultPageSize;
		}
	}
}
