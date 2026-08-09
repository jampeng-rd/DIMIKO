

namespace ECommerce.Models.Payments
{
	public class NewebPayPaymentResponse
	{
		public string Status { get; set; } = string.Empty;

		public string Message { get; set; } = string.Empty;

		public NewebPayTradeResult? Result { get; set; }
	}
}
