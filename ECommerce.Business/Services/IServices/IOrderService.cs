using ECommerce.Models;
using ECommerce.Models.Common;
using ECommerce.Models.ServiceResults;

namespace ECommerce.Business.Services.IServices
{
	public interface IOrderService
	{
		// 前台：建立訂單
		Task<CreateOrderResult> CreateOrderAsync(OrderHeader orderHeader, string userId);


		// 前台：取得單筆訂單
		Task<OrderHeader?> GetOrderByIdAsync(int orderId, string userId);

		// 「金流專用」查詢
		Task<OrderHeader?> GetOrderByIdAsync(int orderId);

		// 前台：建立一次新的付款交易 (每一次付款都會建立一筆交易訂單)
		Task<PaymentTransaction?> CreatePaymentTransactionAsync(int orderId, string userId);


		// 藍新付款成功：更新付款交易與訂單
		Task<bool> MarkPaymentTransactionAsSuccessAsync(
			string merchantOrderNo,
			int amount,
			string tradeNo,
			string paymentType,
			DateTime paymentDate);

		// 藍新付款失敗：更新單次付款交易
		Task<bool> MarkPaymentTransactionAsFailedAsync(
			string merchantOrderNo,
			int amount,
			string? message);


		// 金流回傳：依訂單編號取得訂單
		Task<OrderHeader?> GetOrderByNumberAsync(string orderNumber);


		// 依訂單編號找訂單
		Task<OrderHeader?> GetOrderByNumberAsync(string orderNumber, string userId);


		// 前台：取使用者所有訂單資料
		//Task<IEnumerable<OrderHeader>> GetUserOrdersAsync(string userId);

		// 前台：取得使用者訂單分頁資料
		Task<PagedResult<OrderHeader>> GetUserOrdersAsync(string userId, int pageNumber, int pageSize);


		// 前台：訂單詳細資料
		Task<OrderHeader?> GetUserOrderDetailsAsync(int orderId, string userId);


		// 前台：使用者取消自己的未付款訂單
		Task<bool> CancelUserOrderAsync(int orderId, string userId);


		// 系統：取消逾期未付款訂單並恢復庫存
		Task<int> CancelExpiredOrdersAsync();


		// 後台：取得指定月份每日的訂單數量
		Task<IReadOnlyDictionary<int, int>> GetMonthlyOrderCountsAsync(int year, int month);

		// 後台：取得指定日期的 <分頁資料>
		Task<PagedResult<OrderHeader>> GetOrdersByDateAsync(
			DateTime taiwanDate,
			string? status,
			int pageNumber,
			int pageSize);

		// 後台：計算當天全部訂單總額
		Task<decimal> GetOrderTotalByDateAsync(DateTime taiwanDate, string? status);


		// 後台：取得全部訂單 (未使用)
		Task<IEnumerable<OrderHeader>> GetAllOrdersAsync();

		// 後台：取得單筆完整訂單
		Task<OrderHeader?> GetOrderDetailsByIdAsync(int orderId);

		// 後台：狀態處理- 待確認 -> 確認訂單
		Task<bool> ConfirmOrderAsync(int orderId);

		// 後台：狀態處理- 確認訂單 -> 開始處理
		Task<bool> StartProcessingOrderAsync(int orderId);

		// 後台：狀態處理- 開始處理 -> 已出貨
		Task<bool> ShipOrderAsync(int orderId, string carrier, string trackingNumber);

		// 後台：取消訂單並恢復庫存
		Task<bool> CancelOrderAsync(int orderId);

	}
}
