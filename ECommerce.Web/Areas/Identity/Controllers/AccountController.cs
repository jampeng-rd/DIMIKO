using ECommerce.Models;
using ECommerce.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Identity.Controllers
{
	[Area("Identity")]
	public class AccountController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;

		public AccountController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
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
				if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
				{
					return Redirect(returnUrl);
				}

				return RedirectToAction(
					actionName: "Index",
					controllerName: "Home",
					routeValues: new { area = "Customer" });
			}

			ModelState.AddModelError(string.Empty, "帳號或密碼不正確。");

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
				await _signInManager.SignInAsync(applicationUser, isPersistent: false);

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

			return RedirectToAction("Index", "Home", new { area = "Customer" });
		}
	}
}
