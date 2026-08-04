using ECommerce.Models;
using ECommerce.Models.ViewModels;
using ECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.RoleAdmin)]
	public class UserController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;

		public UserController(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task<IActionResult> Index()
		{
			var users = await _userManager.Users
				.AsNoTracking()
				.OrderBy(user => user.Name)
				.ThenBy(user => user.Email)
				.ToListAsync();

			var userViewModels = new List<UserListViewModel>();

			foreach (var user in users)
			{
				var roles = await _userManager.GetRolesAsync(user);
				var isLockedOut = await _userManager.IsLockedOutAsync(user);

				userViewModels.Add(new UserListViewModel
				{
					Id = user.Id,
					Name = user.Name,
					Email = user.Email ?? string.Empty,
					PhoneNumber = user.PhoneNumber,
					Roles = roles.Count == 0 ? "尚未設定" : string.Join("、", roles),
					IsLockedOut = isLockedOut
				});
			}

			return View(userViewModels);
		}

		public async Task<IActionResult> EditRole(string? id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return NotFound();
			}

			var user = await _userManager.FindByIdAsync(id);

			if (user == null)
			{
				return NotFound();
			}

			var currentRoles = await _userManager.GetRolesAsync(user);

			var model = new UserRoleEditViewModel
			{
				UserId = user.Id,
				Name = user.Name,
				Email = user.Email ?? string.Empty,
				SelectedRole = currentRoles.FirstOrDefault() ?? string.Empty,
				RoleList = GetRoleList()
			};

			return View(model);
		}

		// 更新流程要先移除再加入
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditRole(UserRoleEditViewModel model)
		{
			model.RoleList = GetRoleList();

			if (!ModelState.IsValid)
			{
				return View(model);
			}

			string[] allowedRoles =
			[
				SD.RoleCustomer,
				SD.RoleEmployee,
				SD.RoleAdmin
			];

			if (!allowedRoles.Contains(model.SelectedRole))
			{
				ModelState.AddModelError(nameof(model.SelectedRole), "選擇的角色無效");

				return View(model);
			}

			var user = await _userManager.FindByIdAsync(model.UserId);

			if (user == null)
			{
				return NotFound();
			}

			var currentUserId = _userManager.GetUserId(User);

			if (user.Id == currentUserId && model.SelectedRole != SD.RoleAdmin)
			{
				ModelState.AddModelError(string.Empty, "不能修改目前的管理員角色");

				return View(model);
			}

			var currentRoles = await _userManager.GetRolesAsync(user);

			if (currentRoles.Count == 1 && currentRoles[0] == model.SelectedRole)
			{
				TempData["success"] = "角色沒有變更";

				return RedirectToAction(nameof(Index));
			}

			if (currentRoles.Count > 0)
			{
				var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

				if (!removeResult.Succeeded)
				{
					foreach (var error in removeResult.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}

					return View(model);
				}
			}

			// 加入角色
			var addResult = await _userManager.AddToRoleAsync(user, model.SelectedRole);

			if (!addResult.Succeeded)
			{
				// 加入失敗時嘗試復原舊角色
				if (currentRoles.Count > 0)
				{
					await _userManager.AddToRolesAsync(
						user,
						currentRoles);
				}

				foreach (var error in addResult.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}

				return View(model);
			}

			// 更新使用者的安全戳記，使舊登入狀態在 Identity 下次驗證時失效
			await _userManager.UpdateSecurityStampAsync(user);

			TempData["success"] = "角色更新成功";

			return RedirectToAction(nameof(Index));
		}

		// 帳號停用
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Disable(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return NotFound();
			}

			var user = await _userManager.FindByIdAsync(id);

			if (user == null)
			{
				return NotFound();
			}

			// 防止管理員停用自己
			var currentUserId = _userManager.GetUserId(User);
			if (user.Id == currentUserId)
			{
				TempData["error"] = "不能停用目前的管理員帳號";

				return RedirectToAction(nameof(Index));
			}

			// 停用帳號
			var enableLockoutResult = await _userManager.SetLockoutEnabledAsync(user, true);

			if (!enableLockoutResult.Succeeded)
			{
				TempData["error"] = string.Join(
					"、",
					enableLockoutResult.Errors.Select(error => error.Description));

				return RedirectToAction(nameof(Index));
			}

			var lockoutResult =
				await _userManager.SetLockoutEndDateAsync(
					user,
					DateTimeOffset.MaxValue);

			if (!lockoutResult.Succeeded)
			{
				TempData["error"] = string.Join(
					"、",
					lockoutResult.Errors.Select(error => error.Description));

				return RedirectToAction(nameof(Index));
			}

			// 使該使用者現有的登入狀態在下次安全戳記驗證時失效。
			await _userManager.UpdateSecurityStampAsync(user);

			TempData["success"] = "帳號已停用";

			return RedirectToAction(nameof(Index));
		}

		// 帳號啟用
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Enable(string id)
		{
			if (string.IsNullOrWhiteSpace(id))
			{
				return NotFound();
			}

			var user = await _userManager.FindByIdAsync(id);

			if (user == null)
			{
				return NotFound();
			}

			// 啟用帳號
			var result = await _userManager.SetLockoutEndDateAsync(user, null);

			if (!result.Succeeded)
			{
				TempData["error"] = string.Join(
					"、",
					result.Errors.Select(error => error.Description));

				return RedirectToAction(nameof(Index));
			}

			// 一併清除先前累積的登入失敗次數。
			await _userManager.ResetAccessFailedCountAsync(user);

			await _userManager.UpdateSecurityStampAsync(user);

			TempData["success"] = "帳號已重新啟用";

			return RedirectToAction(nameof(Index));
		}


		// 角色選單方法
		private static IEnumerable<SelectListItem> GetRoleList()
		{
			return
			[
				new SelectListItem
				{
					Text = "一般會員",
					Value = SD.RoleCustomer
				},
				new SelectListItem
				{
					Text = "員工",
					Value = SD.RoleEmployee
				},
				new SelectListItem
				{
					Text = "系統管理員",
					Value = SD.RoleAdmin
				}
			];
		}

	}
}
