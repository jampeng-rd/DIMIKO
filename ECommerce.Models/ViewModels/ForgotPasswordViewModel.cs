using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
	public class ForgotPasswordViewModel
	{
		[Required(ErrorMessage = "請輸入電子郵件")]
		[EmailAddress(ErrorMessage = "電子郵件格式不正確")]
		[Display(Name = "電子郵件")]
		public string Email { get; set; } = string.Empty;
	}
}
