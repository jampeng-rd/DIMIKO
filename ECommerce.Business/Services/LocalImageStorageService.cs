using ECommerce.Business.Services.IServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Business.Services
{
	public class LocalImageStorageService : IImageStorageService
	{
		private static readonly HashSet<string> AllowedExtensions =
			new(StringComparer.OrdinalIgnoreCase)
			{
				".jpg",
				".jpeg",
				".png",
				".webp"
			};
		
		private const long MaxFileSize = 5 * 1024 * 1024;

		private readonly IWebHostEnvironment _environment;

		public LocalImageStorageService(IWebHostEnvironment environment)
		{
			_environment = environment;
		}


		public async Task<SavedImage> SaveImageAsync(
			string folder,
			IFormFile image)
		{
			if (image == null || image.Length == 0)
			{
				throw new InvalidOperationException("請選擇圖片。");
			}

			if (image.Length > MaxFileSize)
			{
				throw new InvalidOperationException($"{image.FileName} 不可超過 5 MB");
			}

			string extension = Path
				.GetExtension(image.FileName)
				.ToLowerInvariant();

			if (!AllowedExtensions.Contains(extension))
			{
				throw new InvalidOperationException($"{image.FileName} 不支援的圖片格式");
			}

			string generatedFileName = $"{Guid.NewGuid():N}{extension}";

			string normalizedFolder = folder
				.Trim()
				.Trim('/')
				.Replace('/', Path.DirectorySeparatorChar);

			string directory = Path.Combine(
				_environment.WebRootPath,
				"images",
				normalizedFolder);

			Directory.CreateDirectory(directory);

			string physicalPath = Path.Combine(directory, generatedFileName);

			await using var stream = new FileStream(physicalPath, FileMode.CreateNew);

			await image.CopyToAsync(stream);

			string imageUrl = $"/images/{folder.Trim().Trim('/')}/{generatedFileName}";

			return new SavedImage
			{
				FileName = generatedFileName,
				ImageUrl = imageUrl
			};
		}


		public Task<bool> DeleteImageAsync(string imageUrl)
		{
			if (string.IsNullOrWhiteSpace(imageUrl))
			{
				return Task.FromResult(false);
			}

			string relativePath = imageUrl
				.TrimStart('/')
				.Replace('/', Path.DirectorySeparatorChar);

			string physicalPath = Path.Combine(_environment.WebRootPath, relativePath);

			if (!File.Exists(physicalPath))
			{
				return Task.FromResult(false);
			}

			File.Delete(physicalPath);

			return Task.FromResult(true);
		}


		public Task<int> DeleteFolderAsync(string folder)
		{
			string normalizedFolder = folder
				.Trim()
				.Trim('/')
				.Replace('/', Path.DirectorySeparatorChar);

			string directory = Path.Combine(
				_environment.WebRootPath,
				"images",
				normalizedFolder);

			if (!Directory.Exists(directory))
			{
				return Task.FromResult(0);
			}

			int fileCount = Directory
				.EnumerateFiles(
					directory,
					"*",
					SearchOption.AllDirectories)
				.Count();

			Directory.Delete(directory, recursive: true);

			return Task.FromResult(fileCount);
		}


	}
}
