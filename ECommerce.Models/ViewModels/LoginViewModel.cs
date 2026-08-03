using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
	public class LoginViewModel
	{
		[Required(ErrorMessage = "請輸入電子郵件")]
		[EmailAddress(ErrorMessage = "電子郵件格式不正確")]
		[Display(Name = "帳號")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "請輸入密碼")]
		[DataType(DataType.Password)]
		[Display(Name = "密碼")]
		public string Password { get; set; } = string.Empty;

		[Display(Name = "Remember me?")]
		public bool RememberMe { get; set; }
	}
}
