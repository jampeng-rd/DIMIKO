using ECommerce.Models;
using ECommerce.Models.ServiceResults;

namespace ECommerce.Business.Services.IServices
{
	public interface IOrderService
	{
		// 前台：建立訂單
		Task<CreateOrderResult> CreateOrderAsync(OrderHeader orderHeader, string userId);

		// 前台：取得單筆訂單
		Task<OrderHeader?> GetOrderByIdAsync(int orderId, string userId);

		// 前台
		Task<IEnumerable<OrderHeader>> GetUserOrdersAsync(string userId);

		// 前台
		Task<OrderHeader?> GetUserOrderDetailsAsync(int orderId, string userId);


		// 後台：取得指定月份每日的訂單數量
		Task<IReadOnlyDictionary<int, int>> GetMonthlyOrderCountsAsync(int year, int month);

		// 後台：取得指定日期的訂單
		Task<IReadOnlyList<OrderHeader>> GetOrdersByDateAsync(DateTime taiwanDate);

		// 後台：取得全部訂單
		Task<IEnumerable<OrderHeader>> GetAllOrdersAsync();

		// 後台：取得單筆完整訂單
		Task<OrderHeader?> GetOrderDetailsByIdAsync(int orderId);

	}
}
