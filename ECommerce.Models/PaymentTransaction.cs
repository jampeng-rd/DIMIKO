using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
	public class PaymentTransaction
	{
		public int Id { get; set; }

		// 所屬訂單
		public int OrderHeaderId { get; set; }


		[ForeignKey(nameof(OrderHeaderId))]
		[ValidateNever]
		public OrderHeader OrderHeader { get; set; } = null!;


		// 每一次送往藍新的 MerchantOrderNo (每次付款都不同的藍新訂單編號)
		[Required]
		[StringLength(30)]
		public string MerchantOrderNo { get; set; } = string.Empty;


		// 本次付款金額
		[Column(TypeName = "decimal(18,2)")]
		public decimal Amount { get; set; }


		// 本次付款狀態
		[Required]
		[StringLength(20)]
		public string Status { get; set; } = string.Empty;


		// 藍新交易編號 (藍新成功後給的 TradeNo)
		[StringLength(100)]
		public string? NewebPayTradeNo { get; set; }


		// 付款方式
		[StringLength(50)]
		public string? PaymentType { get; set; }


		// 藍新失敗訊息
		[StringLength(500)]
		public string? Message { get; set; }


		// 建立本次付款交易的時間
		public DateTime CreatedDate { get; set; }


		// 實際付款成功時間
		public DateTime? PaymentDate { get; set; }
	}
}
