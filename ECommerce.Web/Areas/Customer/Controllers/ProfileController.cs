using ECommerce.Models;
using ECommerce.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class ProfileController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;

		public ProfileController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}


		public async Task<IActionResult> Index()
		{
			var user = await _userManager.GetUserAsync(User);

			if (user == null)
			{
				return NotFound();
			}

			var model = new ProfileViewModel
			{
				Name = user.Name,
				Email = user.Email ?? string.Empty,
				PhoneNumber = user.PhoneNumber,
				City = user.City,
				State = user.State,
				StreetAddress = user.StreetAddress,
				PostalCode = user.PostalCode
			};

			return View(model);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(ProfileViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _userManager.GetUserAsync(User);

			if (user == null)
			{
				return NotFound();
			}

			// 檢查新的 Email 是否已經被其他帳號使用
			var existingUser = await _userManager.FindByEmailAsync(model.Email);

			if (existingUser != null && existingUser.Id != user.Id)
			{
				ModelState.AddModelError(nameof(model.Email), "此電子郵件已經被其他帳號使用");

				return View(model);
			}

			user.Name = model.Name.Trim();
			user.PhoneNumber = model.PhoneNumber?.Trim();
			user.City = model.City?.Trim();
			user.State = model.State?.Trim();
			user.StreetAddress = model.StreetAddress?.Trim();
			user.PostalCode = model.PostalCode?.Trim();

			// Email 有變更時，同步更新 Email 與 UserName
			if (!string.Equals(
				user.Email,
				model.Email,
				StringComparison.OrdinalIgnoreCase))
			{
				var emailResult = await _userManager.SetEmailAsync(user, model.Email.Trim());

				if (!emailResult.Succeeded)
				{
					foreach (var error in emailResult.Errors)
					{
						ModelState.AddModelError(nameof(model.Email), error.Description);
					}

					return View(model);
				}

				var userNameResult = await _userManager.SetUserNameAsync(user, model.Email.Trim());

				if (!userNameResult.Succeeded)
				{
					foreach (var error in userNameResult.Errors)
					{
						ModelState.AddModelError(nameof(model.Email), error.Description);
					}

					return View(model);
				}
			}

			var updateResult = await _userManager.UpdateAsync(user);

			if (!updateResult.Succeeded)
			{
				foreach (var error in updateResult.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}

				return View(model);
			}


			// 重新產生目前登入 Cookie，避免修改 Email / UserName 後登入資訊還停留在舊狀態 
			await _signInManager.RefreshSignInAsync(user);

			TempData["success"] = "資料更新成功";

			return RedirectToAction(nameof(Index));
		}


		public IActionResult ChangePassword()
		{
			return View(new ChangePasswordViewModel());
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _userManager.GetUserAsync(User);

			if (user == null)
			{
				return NotFound();
			}

			var result = await _userManager.ChangePasswordAsync(
				user,
				model.CurrentPassword,
				model.NewPassword);

			if (!result.Succeeded)
			{
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}

				return View(model);
			}

			// 修改密碼後重新整理登入 Cookie
			await _signInManager.RefreshSignInAsync(user);

			TempData["success"] = "密碼修改成功";

			return RedirectToAction(nameof(Index));
		}


	}
}
