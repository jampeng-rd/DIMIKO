using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce.Models
{
	public class ApplicationUser : IdentityUser
	{
		[Required]
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

		[NotMapped]
		[Display(Name = "角色")]
		public string Role { get; set; } = string.Empty;
	}
}
