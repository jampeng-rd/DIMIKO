using ECommerce.Models;
using ECommerce.Models.Payments;

namespace ECommerce.Business.Services.IServices
{
	public interface INewebPayService
	{
		bool IsConfigured();

		NewebPayPaymentRequest CreatePaymentRequest(OrderHeader orderHeader);

		NewebPayPaymentResponse DecryptPaymentResponse(string tradeInfo);

		bool VerifyTradeSha(string tradeInfo, string tradeSha);

		NewebPayPaymentResponse ValidateAndDecryptPaymentResponse(string tradeInfo, string tradeSha);

	}
}
