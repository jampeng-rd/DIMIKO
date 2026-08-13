using ECommerce.Utility.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
	public class ProfileViewModel
	{
		[Required(ErrorMessage = "請輸入姓名")]
		[Display(Name = "姓名")]
		public string Name { get; set; } = string.Empty;


		[Required(ErrorMessage = "請輸入電子郵件")]
		[EmailAddress(ErrorMessage = "電子郵件格式不正確")]
		[Display(Name = "電子郵件")]
		public string Email { get; set; } = string.Empty;


		[TaiwanPhone]
		[Display(Name = "電話號碼")]
		public string? PhoneNumber { get; set; }


		[Display(Name = "縣市")]
		public string? City { get; set; }


		[Display(Name = "區／鄉／鎮／市")]
		public string? State { get; set; }


		[Display(Name = "詳細地址")]
		public string? StreetAddress { get; set; }


		[Display(Name = "郵遞區號")]
		public string? PostalCode { get; set; }
	}
}
