using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Models.ViewModels
{
	public class UserRoleEditViewModel
	{
		[Required]
		public string UserId { get; set; } = string.Empty;

		[Display(Name = "姓名")]
		public string Name { get; set; } = string.Empty;

		[Display(Name = "電子郵件")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "請選擇角色")]
		[Display(Name = "角色")]
		public string SelectedRole { get; set; } = string.Empty;

		[ValidateNever]
		public IEnumerable<SelectListItem> RoleList { get; set; } = Enumerable.Empty<SelectListItem>();
	}
}
