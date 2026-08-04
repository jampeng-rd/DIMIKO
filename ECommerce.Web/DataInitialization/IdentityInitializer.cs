using ECommerce.Models;
using ECommerce.Utility;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Web.DataInitialization
{
	public static class IdentityInitializer
	{
		public static async Task InitializeAsync(
			IServiceProvider serviceProvider,
			IConfiguration configuration)
		{
			var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

			var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

			await CreateRoleIfNotExistsAsync(roleManager, SD.RoleCustomer);
			await CreateRoleIfNotExistsAsync(roleManager, SD.RoleEmployee);
			await CreateRoleIfNotExistsAsync(roleManager, SD.RoleAdmin);

			var adminEmail = configuration["InitialAdmin:Email"];

			var adminPassword = configuration["InitialAdmin:Password"];

			if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
			{
				throw new InvalidOperationException("尚未設定 InitialAdmin:Email 或 InitialAdmin:Password。");
			}

			var adminUser = await userManager.FindByEmailAsync(adminEmail);

			if (adminUser == null)
			{
				adminUser = new ApplicationUser
				{
					UserName = adminEmail,
					Email = adminEmail,
					Name = "系統管理員",
					EmailConfirmed = true
				};

				var createResult = await userManager.CreateAsync(adminUser, adminPassword);

				if (!createResult.Succeeded)
				{
					var errors = string.Join(Environment.NewLine, createResult.Errors.Select(error => error.Description));

					throw new InvalidOperationException($"建立初始管理員失敗：{Environment.NewLine}{errors}");
				}
			}

			if (!await userManager.IsInRoleAsync(adminUser, SD.RoleAdmin))
			{
				var addRoleResult = await userManager.AddToRoleAsync(adminUser, SD.RoleAdmin);

				if (!addRoleResult.Succeeded)
				{
					var errors = string.Join(Environment.NewLine, addRoleResult.Errors.Select(error => error.Description));

					throw new InvalidOperationException($"加入 Admin 角色失敗：{Environment.NewLine}{errors}");
				}
			}
		}

		private static async Task CreateRoleIfNotExistsAsync(RoleManager<IdentityRole> roleManager, string roleName)
		{
			if (await roleManager.RoleExistsAsync(roleName))
			{
				return;
			}

			var result = await roleManager.CreateAsync(new IdentityRole(roleName));

			if (!result.Succeeded)
			{
				var errors = string.Join(Environment.NewLine, result.Errors.Select(error => error.Description));

				throw new InvalidOperationException($"建立角色 {roleName} 失敗：{Environment.NewLine}{errors}");
			}
		}

	}
}
