using ECommerce.Models;
using ECommerce.Models.ServiceResults;

namespace ECommerce.Business.Services.IServices
{
	public interface IOrderService
	{
		// 建立訂單
		Task<CreateOrderResult> CreateOrderAsync(OrderHeader orderHeader, string userId);
		
		// 取得單筆訂單
		Task<OrderHeader?> GetOrderByIdAsync(int orderId, string userId);

		Task<IEnumerable<OrderHeader>> GetUserOrdersAsync(string userId);

		Task<OrderHeader?> GetUserOrderDetailsAsync(int orderId, string userId);


	}
}
