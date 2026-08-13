using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
	public class ResetPasswordViewModel
	{
		[Required]
		public string Token { get; set; } = string.Empty;


		[Required(ErrorMessage = "請輸入電子郵件")]
		[EmailAddress(ErrorMessage = "電子郵件格式不正確")]
		[Display(Name = "電子郵件")]
		public string Email { get; set; } = string.Empty;


		[Required(ErrorMessage = "請輸入新密碼")]
		[DataType(DataType.Password)]
		[Display(Name = "新密碼")]
		public string Password { get; set; } = string.Empty;


		[Required(ErrorMessage = "請再次輸入新密碼")]
		[DataType(DataType.Password)]
		[Compare(nameof(Password), ErrorMessage = "兩次輸入的密碼不一致")]
		[Display(Name = "確認新密碼")]
		public string ConfirmPassword { get; set; } = string.Empty;
	}
}
