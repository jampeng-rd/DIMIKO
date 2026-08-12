using ECommerce.Business.Services.IServices;
using ECommerce.Models;
using ECommerce.Models.Payments;
using ECommerce.Utility.Settings;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ECommerce.Business.Services
{
	public class NewebPayService : INewebPayService
	{
		private readonly NewebPaySettings _settings;

		public NewebPayService(IOptions<NewebPaySettings> options)
		{
			_settings = options.Value;
		}

		public bool IsConfigured()
		{
			return
				!string.IsNullOrWhiteSpace(_settings.MerchantId) &&
				!string.IsNullOrWhiteSpace(_settings.HashKey) &&
				!string.IsNullOrWhiteSpace(_settings.HashIV) &&
				!string.IsNullOrWhiteSpace(_settings.PaymentUrl) &&
				!string.IsNullOrWhiteSpace(_settings.Version) &&
				!string.IsNullOrWhiteSpace(_settings.ReturnUrl) &&
				!string.IsNullOrWhiteSpace(_settings.NotifyUrl);
		}

		public NewebPayPaymentRequest CreatePaymentRequest(OrderHeader orderHeader)
		{
			if (!IsConfigured())
			{
				throw new InvalidOperationException("藍新金流設定不完整");
			}

			if (orderHeader == null)
			{
				throw new ArgumentNullException(nameof(orderHeader));
			}

			if (string.IsNullOrWhiteSpace(orderHeader.OrderNumber))
			{
				throw new ArgumentException("訂單編號不可為空白", nameof(orderHeader));
			}

			if (orderHeader.OrderTotal <= 0)
			{
				throw new ArgumentException("訂單金額必須大於 0", nameof(orderHeader));
			}

			// 建立原始付款資料
			var tradeInfoData = BuildTradeInfo(orderHeader);

			// 把交易資料加密
			var tradeInfo = EncryptTradeInfo(tradeInfoData);

			// 驗證 TradeInfo 有沒有被修改
			var tradeSha = CreateTradeSha(tradeInfo);

			return new NewebPayPaymentRequest
			{
				MerchantId = _settings.MerchantId,
				TradeInfo = tradeInfo,
				TradeSha = tradeSha,
				Version = _settings.Version,
				PaymentUrl = _settings.PaymentUrl
			};
		}

		public NewebPayPaymentResponse DecryptPaymentResponse(string tradeInfo)
		{
			if (!IsConfigured())
			{
				throw new InvalidOperationException("藍新金流設定不完整");
			}

			if (string.IsNullOrWhiteSpace(tradeInfo))
			{
				throw new ArgumentException("TradeInfo 不可為空白", nameof(tradeInfo));
			}

			var decryptedData = DecryptTradeInfo(tradeInfo);

			var response = JsonSerializer.Deserialize<NewebPayPaymentResponse>(decryptedData);

			if (response == null)
			{
				throw new InvalidOperationException("無法解析藍新付款回傳資料");
			}

			return response;
		}

		public bool VerifyTradeSha(string tradeInfo, string tradeSha)
		{
			if (!IsConfigured())
			{
				throw new InvalidOperationException("藍新金流設定不完整");
			}

			if (string.IsNullOrWhiteSpace(tradeInfo) || string.IsNullOrWhiteSpace(tradeSha))
			{
				return false;
			}

			var calculatedTradeSha = CreateTradeSha(tradeInfo);

			return string.Equals(
				calculatedTradeSha,
				tradeSha,
				StringComparison.OrdinalIgnoreCase);
		}

		public NewebPayPaymentResponse ValidateAndDecryptPaymentResponse(string tradeInfo, string tradeSha)
		{
			if (!VerifyTradeSha(tradeInfo, tradeSha))
			{
				throw new InvalidOperationException("藍新付款資料驗證失敗");
			}

			var response = DecryptPaymentResponse(tradeInfo);

			if (response.Result == null)
			{
				throw new InvalidOperationException("藍新付款結果內容不存在");
			}

			if (!string.Equals(
				response.Result.MerchantID,
				_settings.MerchantId,
				StringComparison.Ordinal))
			{
				throw new InvalidOperationException("藍新商店代號驗證失敗");
			}

			return response;
		}


		// 建立原始付款資料
		private string BuildTradeInfo(OrderHeader orderHeader)
		{
			var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

			var amount = decimal.ToInt32(orderHeader.OrderTotal);

			var parameters = new Dictionary<string, string>
			{
				["MerchantID"] = _settings.MerchantId,
				["RespondType"] = "JSON",
				["TimeStamp"] = timeStamp.ToString(),
				["Version"] = _settings.Version,
				["MerchantOrderNo"] = orderHeader.OrderNumber,
				["Amt"] = amount.ToString(),
				["ItemDesc"] = "DIMIKO 商品訂單",

				["ReturnURL"] = _settings.ReturnUrl,
				["NotifyURL"] = _settings.NotifyUrl,

				// 只啟用信用卡一次付清
				["CREDIT"] = "1"
			};

			return string.Join(
				"&",
				parameters.Select(parameter =>
					$"{parameter.Key}={Uri.EscapeDataString(parameter.Value)}"));
		}

		// 送付款資料給藍新
		private string EncryptTradeInfo(string tradeInfo)
		{
			using var aes = Aes.Create();

			aes.Key = Encoding.UTF8.GetBytes(_settings.HashKey);
			aes.IV = Encoding.UTF8.GetBytes(_settings.HashIV);

			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;

			using var encryptor = aes.CreateEncryptor();

			var plainBytes = Encoding.UTF8.GetBytes(tradeInfo);

			var encryptedBytes = encryptor.TransformFinalBlock(
				plainBytes,
				0,
				plainBytes.Length);

			return Convert.ToHexString(encryptedBytes).ToLowerInvariant();
		}

		private string CreateTradeSha(string tradeInfo)
		{
			var rawData =
				$"HashKey={_settings.HashKey}" +
				$"&{tradeInfo}" +
				$"&HashIV={_settings.HashIV}";

			var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));

			return Convert.ToHexString(hashBytes).ToUpperInvariant();
		}

		// 讀取藍新回傳資料
		private string DecryptTradeInfo(string tradeInfo)
		{
			using var aes = Aes.Create();

			aes.Key = Encoding.UTF8.GetBytes(_settings.HashKey);
			aes.IV = Encoding.UTF8.GetBytes(_settings.HashIV);

			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;

			using var decryptor = aes.CreateDecryptor();

			var encryptedBytes = Convert.FromHexString(tradeInfo);

			var decryptedBytes =
				decryptor.TransformFinalBlock(
					encryptedBytes,
					0,
					encryptedBytes.Length);

			return Encoding.UTF8.GetString(decryptedBytes);
		}
	
	}
}
