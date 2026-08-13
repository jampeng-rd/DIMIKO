using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
	public class UserListViewModel
	{
		public string Id { get; set; } = string.Empty;

		[Display(Name = "姓名")]
		public string Name { get; set; } = string.Empty;

		[Display(Name = "電子郵件")]
		public string Email { get; set; } = string.Empty;

		[Display(Name = "電話號碼")]
		public string? PhoneNumber { get; set; }

		[Display(Name = "角色")]
		public string Roles { get; set; } = string.Empty;

		[Display(Name = "帳號狀態")]
		public bool IsLockedOut { get; set; }

		// 保護預設系統管理員帳號
		public bool IsProtected { get; set; }
	}
}
