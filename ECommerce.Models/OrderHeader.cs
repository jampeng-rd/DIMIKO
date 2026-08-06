using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
	public class OrderHeader
	{
		public int Id { get; set; }

		public string ApplicationUserId { get; set; } = string.Empty;

		[ForeignKey(nameof(ApplicationUserId))]
		[ValidateNever]
		public ApplicationUser ApplicationUser { get; set; } = null!;

		public DateTime OrderDate { get; set; }

		public DateTime? ShippingDate { get; set; }

		[Column(TypeName = "decimal(18,2)")]
		public decimal OrderTotal { get; set; }

		[Required]
		[StringLength(50)]
		public string OrderStatus { get; set; } = string.Empty;

		[StringLength(100)]
		public string? TrackingNumber { get; set; }

		[StringLength(100)]
		public string? Carrier { get; set; }


		[StringLength(255)]
		public string? SessionId { get; set; }

		[StringLength(255)]
		public string? PaymentIntentId { get; set; }

		[Required]
		[StringLength(50)]
		public string PaymentStatus { get; set; } = string.Empty;


		[Required(ErrorMessage = "請輸入收件人電話")]
		[StringLength(30)]
		public string PhoneNumber { get; set; } = string.Empty;

		[Required(ErrorMessage = "請輸入城市")]
		[StringLength(100)]
		public string City { get; set; } = string.Empty;

		[Required(ErrorMessage = "請輸入縣市或地區")]
		[StringLength(100)]
		public string State { get; set; } = string.Empty;

		[Required(ErrorMessage = "請輸入收件地址")]
		[StringLength(200)]
		public string StreetAddress { get; set; } = string.Empty;

		[Required(ErrorMessage = "請輸入郵遞區號")]
		[StringLength(20)]
		public string PostalCode { get; set; } = string.Empty;

		[Required(ErrorMessage = "請輸入收件人姓名")]
		[StringLength(100)]
		public string Name { get; set; } = string.Empty;

		[ValidateNever]
		public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
	}
}
