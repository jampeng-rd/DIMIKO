using ECommerce.Business.Services.IServices;
using ECommerce.Models;
using ECommerce.Models.ViewModels;
using ECommerce.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Identity.Controllers
{
	[Area("Identity")]
	public class AccountController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly IShoppingCartService _shoppingCartService;
		private readonly IEmailSenderService _emailSenderService;

		public AccountController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			IShoppingCartService shoppingCartService,
			IEmailSenderService emailSenderService)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_shoppingCartService = shoppingCartService;
			_emailSenderService = emailSenderService;
		}


		public IActionResult Login(string? returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;
			return View();
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel loginViewModel, string? returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;

			if (!ModelState.IsValid)
			{
				return View(loginViewModel);
			}

			var result = await _signInManager.PasswordSignInAsync(
				userName: loginViewModel.Email,
				password: loginViewModel.Password,
				isPersistent: loginViewModel.RememberMe,
				lockoutOnFailure: false);

			if (result.Succeeded)
			{
				var applicationUser = await _userManager.FindByEmailAsync(loginViewModel.Email.Trim());

				if (applicationUser != null)
				{
					var cartCount = await _shoppingCartService.GetCartCountAsync(applicationUser.Id);

					HttpContext.Session.SetInt32(SD.SessionCart, cartCount);
				}
				else
				{
					HttpContext.Session.Remove(SD.SessionCart);
				}

				if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
				{
					return Redirect(returnUrl);
				}

				return RedirectToAction(
					actionName: "Index",
					controllerName: "Home",
					routeValues: new { area = "Customer" });
			}

			if (result.IsLockedOut)
			{
				ModelState.AddModelError(string.Empty, "此帳號已被停用，請聯絡客服或管理員");

				return View(loginViewModel);
			}

			ModelState.AddModelError(string.Empty, "帳號或密碼不正確");

			return View(loginViewModel);
		}


		public IActionResult Register(string? returnUrl = null)
		{
			var model = new RegisterViewModel();

			ViewData["ReturnUrl"] = returnUrl;

			return View(model); ;
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel registerViewModel, string? returnUrl = null)
		{
			ViewData["ReturnUrl"] = returnUrl;

			if (!ModelState.IsValid)
			{
				return View(registerViewModel);
			}

			var applicationUser = new ApplicationUser
			{
				UserName = registerViewModel.Email.Trim(),
				Email = registerViewModel.Email.Trim(),
				Name = registerViewModel.Name.Trim(),

				PhoneNumber = string.IsNullOrWhiteSpace(registerViewModel.PhoneNumber)
					? null
					: registerViewModel.PhoneNumber.Trim(),

				City = string.IsNullOrWhiteSpace(registerViewModel.City)
					? null
					: registerViewModel.City.Trim(),

				State = string.IsNullOrWhiteSpace(registerViewModel.State)
					? null
					: registerViewModel.State.Trim(),

				StreetAddress = string.IsNullOrWhiteSpace(registerViewModel.StreetAddress)
					? null
					: registerViewModel.StreetAddress.Trim(),

				PostalCode = string.IsNullOrWhiteSpace(registerViewModel.PostalCode)
					? null
					: registerViewModel.PostalCode.Trim()
			};

			var result = await _userManager.CreateAsync(applicationUser, registerViewModel.Password);

			if (result.Succeeded)
			{
				var roleResult = await _userManager.AddToRoleAsync(applicationUser, SD.RoleCustomer);

				// 加入角色失敗時要刪除使用者
				if (!roleResult.Succeeded)
				{
					await _userManager.DeleteAsync(applicationUser);

					foreach (var error in roleResult.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}

					return View(registerViewModel);
				}

				// 註冊完成並自動登入時初始化 Session
				await _signInManager.SignInAsync(applicationUser, isPersistent: false);
				HttpContext.Session.SetInt32(SD.SessionCart, 0);

				if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
				{
					return Redirect(returnUrl);
				}

				return RedirectToAction(
					actionName: "Index",
					controllerName: "Home",
					routeValues: new { area = "Customer" });
			}

			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return View(registerViewModel);
		}


		public IActionResult AccessDenied()
		{
			return View();
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();

			HttpContext.Session.Remove(SD.SessionCart);

			return RedirectToAction("Index", "Home", new { area = "Customer" });
		}



		// ================ 忘記密碼流程 ================ //
		public IActionResult ForgotPassword()
		{
			return View(new ForgotPasswordViewModel());
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var email = model.Email.Trim();

			var user = await _userManager.FindByEmailAsync(email);

			// 不論帳號是否存在，都回相同頁面，
			// 避免透過忘記密碼功能判斷會員 Email 是否存在。
			if (user == null)
			{
				return RedirectToAction(nameof(ForgotPasswordConfirmation));
			}

			var token = await _userManager.GeneratePasswordResetTokenAsync(user);

			var resetUrl = Url.Action(
				action: nameof(ResetPassword),
				controller: "Account",
				values: new
				{
					area = "Identity",
					email = email,
					token = token
				},
				protocol: Request.Scheme);


			if (string.IsNullOrWhiteSpace(resetUrl))
			{
				ModelState.AddModelError(string.Empty, "無法產生密碼重設連結");

				return View(model);
			}


			var emailBody = $"""
				<h2>DIMIKO 密碼重設</h2>

				<p>您好：</p>

				<p>
					我們收到您的密碼重設要求。
				</p>

				<p>
					請點擊以下連結設定新的密碼：
				</p>

				<p>
					<a href="{resetUrl}">
						重設密碼
					</a>
				</p>

				<p>
					如果您沒有提出密碼重設要求，可以忽略這封信。
				</p>

				<p>
					DIMIKO
				</p>
				""";


			await _emailSenderService.SendEmailAsync(
				email,
				"DIMIKO 密碼重設",
				emailBody);


			return RedirectToAction(
				nameof(ForgotPasswordConfirmation));
		}


		public IActionResult ForgotPasswordConfirmation()
		{
			return View();
		}


		public IActionResult ResetPassword(string? email, string? token)
		{
			if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
			{
				return BadRequest();
			}

			var model = new ResetPasswordViewModel
			{
				Email = email,
				Token = token
			};

			return View(model);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _userManager.FindByEmailAsync(model.Email.Trim());

			if (user == null)
			{
				return RedirectToAction(nameof(ResetPasswordConfirmation));
			}

			var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

			if (!result.Succeeded)
			{
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}

				return View(model);
			}

			return RedirectToAction(nameof(ResetPasswordConfirmation));
		}


		public IActionResult ResetPasswordConfirmation()
		{
			return View();
		}

	}
}
