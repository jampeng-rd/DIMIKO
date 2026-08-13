using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
	public class ChangePasswordViewModel
	{
		[Required(ErrorMessage = "請輸入目前密碼")]
		[DataType(DataType.Password)]
		[Display(Name = "目前密碼")]
		public string CurrentPassword { get; set; } = string.Empty;


		[Required(ErrorMessage = "請輸入新密碼")]
		[DataType(DataType.Password)]
		[Display(Name = "新密碼")]
		public string NewPassword { get; set; } = string.Empty;


		[Required(ErrorMessage = "請再次輸入新密碼")]
		[DataType(DataType.Password)]
		[Compare(nameof(NewPassword), ErrorMessage = "兩次輸入的新密碼不一致")]
		[Display(Name = "確認新密碼")]
		public string ConfirmPassword { get; set; } = string.Empty;
	}
}
