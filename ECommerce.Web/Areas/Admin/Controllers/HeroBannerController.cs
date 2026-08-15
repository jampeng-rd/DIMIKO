using ECommerce.Business.Services.IServices;
using ECommerce.Models;
using ECommerce.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class HeroBannerController : Controller
	{
		private readonly IHeroBannerService _heroBannerService;
		private readonly IImageStorageService _imageStorageService;

		public HeroBannerController(
			IHeroBannerService heroBannerService,
			IImageStorageService imageStorageService)
		{
			_heroBannerService = heroBannerService;
			_imageStorageService = imageStorageService;
		}


		public async Task<IActionResult> Index()
		{
			var heroBanners = await _heroBannerService.GetAllHeroBannersAsync();

			return View(heroBanners.ToList());
		}

		public IActionResult Create()
		{
			return View(new HeroBannerCreateViewModel());
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(HeroBannerCreateViewModel viewModel)
		{
			if (!ModelState.IsValid)
			{
				return View(viewModel);
			}

			SavedImage? desktopImage = null;
			SavedImage? mobileImage = null;

			try
			{
				// Desktop
				desktopImage = await _imageStorageService.SaveImageAsync("hero", viewModel.ImageFile!);

				// Mobile
				if (viewModel.MobileImageFile != null &&
					viewModel.MobileImageFile.Length > 0)
				{
					mobileImage = await _imageStorageService.SaveImageAsync("hero", viewModel.MobileImageFile);
				}

				var heroBanner = new HeroBanner
				{
					Title = viewModel.Title,
					Description = viewModel.Description,
					ButtonText = viewModel.ButtonText,
					LinkUrl = viewModel.LinkUrl,
					DisplayOrder = viewModel.DisplayOrder,
					IsActive = viewModel.IsActive,

					ImageUrl = desktopImage.ImageUrl,
					MobileImageUrl = mobileImage?.ImageUrl
				};

				await _heroBannerService.CreateHeroBannerAsync(heroBanner);

				TempData["success"] = "輪播圖新增成功";

				return RedirectToAction(nameof(Index));
			}
			catch (InvalidOperationException exception)
			{
				if (desktopImage != null)
				{
					await _imageStorageService.DeleteImageAsync(desktopImage.ImageUrl);
				}

				if (mobileImage != null)
				{
					await _imageStorageService.DeleteImageAsync(mobileImage.ImageUrl);
				}

				ModelState.AddModelError(nameof(viewModel.ImageFile), exception.Message);

				return View(viewModel);
			}
			catch
			{
				if (desktopImage != null)
				{
					await _imageStorageService.DeleteImageAsync(desktopImage.ImageUrl);
				}

				if (mobileImage != null)
				{
					await _imageStorageService.DeleteImageAsync(mobileImage.ImageUrl);
				}

				throw;
			}

		}


		public async Task<IActionResult> Update(int? id)
		{
			if (id is null or 0)
			{
				return NotFound();
			}

			var heroBanner = await _heroBannerService.GetHeroBannerByIdAsync(id.Value);

			if (heroBanner == null)
			{
				return NotFound();
			}

			var viewModel = new HeroBannerUpdateViewModel
			{
				Id = heroBanner.Id,

				CurrentImageUrl = heroBanner.ImageUrl,
				CurrentMobileImageUrl = heroBanner.MobileImageUrl,

				Title = heroBanner.Title,
				Description = heroBanner.Description,
				ButtonText = heroBanner.ButtonText,
				LinkUrl = heroBanner.LinkUrl,
				DisplayOrder = heroBanner.DisplayOrder,
				IsActive = heroBanner.IsActive
			};

			return View(viewModel);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Update(HeroBannerUpdateViewModel viewModel)
		{
			if (!ModelState.IsValid)
			{
				return View(viewModel);
			}

			var heroBanner = await _heroBannerService.GetHeroBannerByIdAsync(viewModel.Id);

			if (heroBanner == null)
			{
				return NotFound();
			}

			string oldDesktopImageUrl = heroBanner.ImageUrl;
			string? oldMobileImageUrl = heroBanner.MobileImageUrl;

			SavedImage? newDesktopImage = null;
			SavedImage? newMobileImage = null;


			try
			{
				// Desktop
				if (viewModel.ImageFile != null &&
					viewModel.ImageFile.Length > 0)
				{
					newDesktopImage = await _imageStorageService.SaveImageAsync("hero", viewModel.ImageFile);

					heroBanner.ImageUrl = newDesktopImage.ImageUrl;
				}

				// Mobile
				if (viewModel.MobileImageFile != null &&
					viewModel.MobileImageFile.Length > 0)
				{
					newMobileImage = await _imageStorageService.SaveImageAsync("hero", viewModel.MobileImageFile);

					heroBanner.MobileImageUrl = newMobileImage.ImageUrl;
				}

				heroBanner.Title = viewModel.Title;
				heroBanner.Description = viewModel.Description;
				heroBanner.ButtonText = viewModel.ButtonText;
				heroBanner.LinkUrl = viewModel.LinkUrl;
				heroBanner.DisplayOrder = viewModel.DisplayOrder;
				heroBanner.IsActive = viewModel.IsActive;

				await _heroBannerService.UpdateHeroBannerAsync(heroBanner);

				// DB 更新成功後，才刪除被取代的舊圖片
				if (newDesktopImage != null)
				{
					await _imageStorageService.DeleteImageAsync(oldDesktopImageUrl);
				}

				if (newMobileImage != null && !string.IsNullOrWhiteSpace(oldMobileImageUrl))
				{
					await _imageStorageService.DeleteImageAsync(oldMobileImageUrl);
				}

				TempData["success"] = "輪播圖更新成功";

				return RedirectToAction(nameof(Index));
			}
			catch (InvalidOperationException exception)
			{
				if (newDesktopImage != null)
				{
					await _imageStorageService.DeleteImageAsync(newDesktopImage.ImageUrl);
				}

				if (newMobileImage != null)
				{
					await _imageStorageService.DeleteImageAsync(newMobileImage.ImageUrl);
				}

				viewModel.CurrentImageUrl = oldDesktopImageUrl;

				viewModel.CurrentMobileImageUrl = oldMobileImageUrl;

				ModelState.AddModelError(string.Empty, exception.Message);

				return View(viewModel);
			}
			catch
			{
				if (newDesktopImage != null)
				{
					await _imageStorageService.DeleteImageAsync(newDesktopImage.ImageUrl);
				}

				if (newMobileImage != null)
				{
					await _imageStorageService.DeleteImageAsync(newMobileImage.ImageUrl);
				}

				throw;
			}

		}


		public async Task<IActionResult> Delete(int? id)
		{
			if (id is null or 0)
			{
				return NotFound();
			}

			var heroBanner = await _heroBannerService.GetHeroBannerByIdAsync(id.Value);

			if (heroBanner == null)
			{
				return NotFound();
			}

			return View(heroBanner);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		[ActionName("Delete")]
		public async Task<IActionResult> DeletePOST(int id)
		{
			var heroBanner = await _heroBannerService.GetHeroBannerByIdAsync(id);

			if (heroBanner == null)
			{
				return NotFound();
			}

			string imageUrl = heroBanner.ImageUrl;
			string? mobileImageUrl = heroBanner.MobileImageUrl;

			await _heroBannerService.DeleteHeroBannerAsync(id);

			await _imageStorageService.DeleteImageAsync(imageUrl);

			if (!string.IsNullOrWhiteSpace(mobileImageUrl))
			{
				await _imageStorageService.DeleteImageAsync(mobileImageUrl);
			}

			TempData["success"] = "輪播圖刪除成功";

			return RedirectToAction(nameof(Index));
		}

	}
}
