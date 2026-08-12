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
				ModelState.AddModelError(nameof(viewModel.ImageFile), "圖片大小不可超過 5 MB");
			}

			var allowedExtensions = new[]
			{
				".jpg",
				".jpeg",
				".png",
				".webp"
			};

			var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

			if (!allowedExtensions.Contains(extension))
			{
				ModelState.AddModelError(nameof(viewModel.ImageFile), "只允許上傳 JPG、JPEG、PNG、WEBP 圖片");
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

			var fileName = $"{Guid.NewGuid()}{extension}";

			var filePath = Path.Combine(heroFolder, fileName);

			await using (var fileStream = new FileStream(filePath, FileMode.Create))
			{
				await imageFile.CopyToAsync(fileStream);
			}

			var heroBanner = new HeroBanner
			{
				Title = viewModel.Title,
				Description = viewModel.Description,
				ButtonText = viewModel.ButtonText,
				LinkUrl = viewModel.LinkUrl,
				DisplayOrder = viewModel.DisplayOrder,
				IsActive = viewModel.IsActive,
				ImageUrl = $"/images/hero/{fileName}"
			};

			try
			{
				await _heroBannerService.CreateHeroBannerAsync(heroBanner);

				TempData["success"] = "輪播圖新增成功";

				return RedirectToAction(nameof(Index));
			}
			catch
			{
				if (System.IO.File.Exists(filePath))
				{
					System.IO.File.Delete(filePath);
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

			string? newFilePath = null;
			string? oldImageUrl = null;

			if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
			{
				const long maxFileSize = 5 * 1024 * 1024;

				var imageFile = viewModel.ImageFile;

				if (imageFile.Length > maxFileSize)
				{
					ModelState.AddModelError(nameof(viewModel.ImageFile), "圖片大小不可超過 5 MB");
				}

				var allowedExtensions = new[]
				{
					".jpg",
					".jpeg",
					".png",
					".webp"
				};

				var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

				if (!allowedExtensions.Contains(extension))
				{
					ModelState.AddModelError(nameof(viewModel.ImageFile), "只允許上傳 JPG、JPEG、PNG、WEBP 圖片");
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

				var fileName = $"{Guid.NewGuid()}{extension}";

				newFilePath = Path.Combine(heroFolder, fileName);

				await using (var fileStream = new FileStream(newFilePath, FileMode.Create))
				{
					await imageFile.CopyToAsync(fileStream);
				}

				oldImageUrl = heroBanner.ImageUrl;

				heroBanner.ImageUrl = $"/images/hero/{fileName}";
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

				if (!string.IsNullOrWhiteSpace(oldImageUrl))
				{
					DeleteHeroImage(oldImageUrl);
				}

				TempData["success"] = "輪播圖更新成功";

				return RedirectToAction(nameof(Index));
			}
			catch
			{
				if (!string.IsNullOrWhiteSpace(newFilePath) && System.IO.File.Exists(newFilePath))
				{
					System.IO.File.Delete(newFilePath);
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

			await _heroBannerService.DeleteHeroBannerAsync(id);

			DeleteHeroImage(imageUrl);

			TempData["success"] = "刪除成功";

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
