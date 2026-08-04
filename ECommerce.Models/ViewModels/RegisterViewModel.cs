using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
	public class RegisterViewModel
	{
		[Required(ErrorMessage = "請輸入電子郵件")]
		[EmailAddress(ErrorMessage = "電子郵件格式不正確")]
		[Display(Name = "電子郵件")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "請輸入密碼")]
		[DataType(DataType.Password)]
		[Display(Name = "密碼")]
		public string Password { get; set; } = string.Empty;

		[Required(ErrorMessage = "請再次輸入密碼")]
		[DataType(DataType.Password)]
		[Display(Name = "確認密碼")]
		[Compare(nameof(Password), ErrorMessage = "密碼與確認密碼不一致")]
		public string ConfirmPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "請輸入姓名")]
		[Display(Name = "姓名")]
		public string Name { get; set; } = string.Empty;

		[Display(Name = "縣市")]
		public string? City { get; set; }

		[Display(Name = "區／鄉／鎮／市")]
		public string? State { get; set; }

		[Display(Name = "詳細地址")]
		public string? StreetAddress { get; set; }

		[Display(Name = "郵遞區號")]
		public string? PostalCode { get; set; }

		[Phone(ErrorMessage = "電話號碼格式不正確")]
		[Display(Name = "電話號碼")]
		public string? PhoneNumber { get; set; }

		//public string? Role { get; set; }

		//[ValidateNever]
		//public IEnumerable<SelectListItem> RoleList { get; set; } = Enumerable.Empty<SelectListItem>();
	}
}
