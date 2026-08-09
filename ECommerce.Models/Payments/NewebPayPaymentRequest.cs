namespace ECommerce.Models.Payments
{
	// 付款要送給藍新的資料
	public class NewebPayPaymentRequest
	{
		public string MerchantId { get; set; } = string.Empty;

		public string TradeInfo { get; set; } = string.Empty;

		public string TradeSha { get; set; } = string.Empty;

		public string Version { get; set; } = string.Empty;

		public string PaymentUrl { get; set; } = string.Empty;
	}
}
