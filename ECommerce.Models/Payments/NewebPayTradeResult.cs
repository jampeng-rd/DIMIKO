namespace ECommerce.Models.Payments
{
	public class NewebPayTradeResult
	{
		public string MerchantID { get; set; } = string.Empty;

		// 驗證金額
		public int Amt { get; set; }

		// 藍新交易編號
		public string TradeNo { get; set; } = string.Empty;

		// 我們自己的訂單
		public string MerchantOrderNo { get; set; } = string.Empty;

		// 付款方式
		public string PaymentType { get; set; } = string.Empty;

		// 付款時間
		public string PayTime { get; set; } = string.Empty;
	}
}
