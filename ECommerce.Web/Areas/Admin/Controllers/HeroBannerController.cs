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
		private readonly IWebHostEnvironment _webHostEnvironment;

		public HeroBannerController(
			IHeroBannerService heroBannerService,
			IWebHostEnvironment webHostEnvironment)
		{
			_heroBannerService = heroBannerService;
			_webHostEnvironment = webHostEnvironment;
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

			var imageFile = viewModel.ImageFile!;

			const long maxFileSize = 5 * 1024 * 1024;
		
			if (imageFile.Length > maxFileSize)
			{
				ModelState.AddModelError(nameof(viewModel.ImageFile), "桌面版圖片大小不可超過 5 MB");
			}

			var allowedExtensions = new[]
			{
				".jpg",
				".jpeg",
				".png",
				".webp"
			};

			var desktopExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

			if (!allowedExtensions.Contains(desktopExtension))
			{
				ModelState.AddModelError(nameof(viewModel.ImageFile), "桌面版圖片只允許 JPG、JPEG、PNG、WEBP");
			}

			// 手機格式
			if (viewModel.MobileImageFile != null && viewModel.MobileImageFile.Length > 0)
			{
				if (viewModel.MobileImageFile.Length > maxFileSize)
				{
					ModelState.AddModelError(nameof(viewModel.MobileImageFile), "手機版圖片大小不可超過 5 MB");
				}

				var mobileExtension = Path.GetExtension(viewModel.MobileImageFile.FileName).ToLowerInvariant();

				if (!allowedExtensions.Contains(mobileExtension))
				{
					ModelState.AddModelError(nameof(viewModel.MobileImageFile), "手機版圖片只允許 JPG、JPEG、PNG、WEBP");
				}
			}


			if (!ModelState.IsValid)
			{
				return View(viewModel);
			}

			var heroFolder = Path.Combine(
				_webHostEnvironment.WebRootPath,
				"images",
				"hero");

			Directory.CreateDirectory(heroFolder);

			// Desktop
			var desktopFileName = $"{Guid.NewGuid()}{desktopExtension}";

			var desktopFilePath = Path.Combine(heroFolder, desktopFileName);

			await using (var fileStream = new FileStream(desktopFilePath, FileMode.Create))
			{
				await imageFile.CopyToAsync(fileStream);
			}

			string? mobileFilePath = null;
			string? mobileImageUrl = null;


			// Mobile
			if (viewModel.MobileImageFile != null && viewModel.MobileImageFile.Length > 0)
			{
				var mobileExtension = Path.GetExtension(viewModel.MobileImageFile.FileName).ToLowerInvariant();

				var mobileFileName = $"{Guid.NewGuid()}{mobileExtension}";

				mobileFilePath = Path.Combine(heroFolder, mobileFileName);

				await using (var fileStream = new FileStream(mobileFilePath, FileMode.Create))
				{
					await viewModel.MobileImageFile.CopyToAsync(fileStream);
				}

				mobileImageUrl = $"/images/hero/{mobileFileName}";
			}


			var heroBanner = new HeroBanner
			{
				Title = viewModel.Title,
				Description = viewModel.Description,
				ButtonText = viewModel.ButtonText,
				LinkUrl = viewModel.LinkUrl,
				DisplayOrder = viewModel.DisplayOrder,
				IsActive = viewModel.IsActive,

				ImageUrl = $"/images/hero/{desktopFileName}",
				MobileImageUrl = mobileImageUrl
			};

			try
			{
				await _heroBannerService.CreateHeroBannerAsync(heroBanner);

				TempData["success"] = "輪播圖新增成功";

				return RedirectToAction(nameof(Index));
			}
			catch
			{
				if (System.IO.File.Exists(desktopFilePath))
				{
					System.IO.File.Delete(desktopFilePath);
				}

				if (!string.IsNullOrWhiteSpace(mobileFilePath) && System.IO.File.Exists(mobileFilePath))
				{
					System.IO.File.Delete(mobileFilePath);
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

			const long maxFileSize = 5 * 1024 * 1024;

			var allowedExtensions = new[]
					{
				".jpg",
				".jpeg",
				".png",
				".webp"
			};

			// 驗證 Desktop
			if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
			{
				if (viewModel.ImageFile.Length > maxFileSize)
				{
					ModelState.AddModelError(nameof(viewModel.ImageFile), "桌面版圖片大小不可超過 5 MB");
				}

				var extension = Path.GetExtension(viewModel.ImageFile.FileName).ToLowerInvariant();

				if (!allowedExtensions.Contains(extension))
				{
					ModelState.AddModelError(nameof(viewModel.ImageFile), "桌面版圖片只允許 JPG、JPEG、PNG、WEBP");
				}
			}

			// 驗證 Mobile
			if (viewModel.MobileImageFile != null && viewModel.MobileImageFile.Length > 0)
			{
				if (viewModel.MobileImageFile.Length > maxFileSize)
				{
					ModelState.AddModelError(nameof(viewModel.MobileImageFile), "手機版圖片大小不可超過 5 MB");
				}

				var extension = Path.GetExtension(viewModel.MobileImageFile.FileName).ToLowerInvariant();

				if (!allowedExtensions.Contains(extension))
				{
					ModelState.AddModelError(nameof(viewModel.MobileImageFile), "手機版圖片只允許 JPG、JPEG、PNG、WEBP");
				}
			}


			if (!ModelState.IsValid)
			{
				viewModel.CurrentImageUrl = heroBanner.ImageUrl;
				viewModel.CurrentMobileImageUrl = heroBanner.MobileImageUrl;

				return View(viewModel);
			}

			var heroFolder = Path.Combine(
				_webHostEnvironment.WebRootPath,
				"images",
				"hero");

			Directory.CreateDirectory(heroFolder);

			string? newDesktopFilePath = null;
			string? newMobileFilePath = null;

			string? oldDesktopImageUrl = null;
			string? oldMobileImageUrl = null;


			// 更新 Desktop
			if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
			{
				var extension = Path.GetExtension(viewModel.ImageFile.FileName).ToLowerInvariant();
				var fileName = $"{Guid.NewGuid()}{extension}";

				newDesktopFilePath = Path.Combine(heroFolder, fileName);

				await using (var fileStream = new FileStream(newDesktopFilePath, FileMode.Create))
				{
					await viewModel.ImageFile.CopyToAsync(fileStream);
				}

				oldDesktopImageUrl = heroBanner.ImageUrl;

				heroBanner.ImageUrl = $"/images/hero/{fileName}";
			}

			// 更新 Mobile
			if (viewModel.MobileImageFile != null && viewModel.MobileImageFile.Length > 0)
			{
				var extension = Path.GetExtension(viewModel.MobileImageFile.FileName).ToLowerInvariant();
				var fileName = $"{Guid.NewGuid()}{extension}";

				newMobileFilePath = Path.Combine(heroFolder, fileName);

				await using (var fileStream = new FileStream(newMobileFilePath, FileMode.Create))
				{
					await viewModel.MobileImageFile.CopyToAsync(fileStream);
				}

				oldMobileImageUrl = heroBanner.MobileImageUrl;

				heroBanner.MobileImageUrl = $"/images/hero/{fileName}";
			}

			heroBanner.Title = viewModel.Title;
			heroBanner.Description = viewModel.Description;
			heroBanner.ButtonText = viewModel.ButtonText;
			heroBanner.LinkUrl = viewModel.LinkUrl;
			heroBanner.DisplayOrder = viewModel.DisplayOrder;
			heroBanner.IsActive = viewModel.IsActive;


			try
			{
				await _heroBannerService.UpdateHeroBannerAsync(heroBanner);

				if (!string.IsNullOrWhiteSpace(oldDesktopImageUrl))
				{
					DeleteHeroImage(oldDesktopImageUrl);
				}

				if (!string.IsNullOrWhiteSpace(oldMobileImageUrl))
				{
					DeleteHeroImage(oldMobileImageUrl);
				}

				TempData["success"] = "輪播圖更新成功";

				return RedirectToAction(nameof(Index));
			}
			catch
			{
				if (!string.IsNullOrWhiteSpace(newDesktopFilePath) && System.IO.File.Exists(newDesktopFilePath))
				{
					System.IO.File.Delete(newDesktopFilePath);
				}

				if (!string.IsNullOrWhiteSpace(newMobileFilePath) && System.IO.File.Exists(newMobileFilePath))
				{
					System.IO.File.Delete(newMobileFilePath);
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

			var imageUrl = heroBanner.ImageUrl;
			var mobileImageUrl = heroBanner.MobileImageUrl;

			await _heroBannerService.DeleteHeroBannerAsync(id);

			DeleteHeroImage(imageUrl);

			if (!string.IsNullOrWhiteSpace(mobileImageUrl))
			{
				DeleteHeroImage(mobileImageUrl);
			}

			TempData["success"] = "輪播圖刪除成功";

			return RedirectToAction(nameof(Index));
		}


		private void DeleteHeroImage(string imageUrl)
		{
			if (string.IsNullOrWhiteSpace(imageUrl))
			{
				return;
			}

			var relativePath = imageUrl
				.TrimStart('/')
				.Replace('/', Path.DirectorySeparatorChar);

			var filePath = Path.Combine(
				_webHostEnvironment.WebRootPath,
				relativePath);

			if (System.IO.File.Exists(filePath))
			{
				System.IO.File.Delete(filePath);
			}
		}

	}
}
