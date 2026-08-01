using ECommerce.Business.Services.IServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Business.Services
{
	public class ProductImageFileService : IProductImageFileService
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

		public ProductImageFileService(IWebHostEnvironment environment)
		{
			_environment = environment;
		}


		public async Task<IReadOnlyList<SavedProductImage>> SaveImagesAsync(
			int productId, 
			IEnumerable<IFormFile> images)
		{
			var savedImages = new List<SavedProductImage>();

			string productDirectory = Path.Combine(
				_environment.WebRootPath,
				"images",
				"products",
				productId.ToString());

			Directory.CreateDirectory(productDirectory);

			foreach (var image in images)
			{
				if (image.Length == 0)
				{
					continue;
				}

				if (image.Length > MaxFileSize)
				{
					throw new InvalidOperationException(
						$"{image.FileName} 不可超過 5 MB");
				}

				string extension = Path
					.GetExtension(image.FileName)
					.ToLowerInvariant();

				if (!AllowedExtensions.Contains(extension))
				{
					throw new InvalidOperationException($"{image.FileName} 不支援的圖片格式");
				}

				string generatedFileName = $"{Guid.NewGuid():N}{extension}";

				string physicalPath = Path.Combine(productDirectory, generatedFileName);

				await using var stream = new FileStream(physicalPath, FileMode.CreateNew);

				await image.CopyToAsync(stream);

				savedImages.Add(new SavedProductImage
				{
					FileName = generatedFileName,
					ImageUrl = $"/images/products/{productId}/{generatedFileName}"
				});
			}

			return savedImages;
		}

		public bool DeleteImage(string imageUrl)
		{
			if (string.IsNullOrWhiteSpace(imageUrl))
			{
				return false;
			}

			string relativePath = imageUrl
				.TrimStart('/')
				.Replace('/', Path.DirectorySeparatorChar);

			string physicalPath = Path.Combine(_environment.WebRootPath, relativePath);

			if (!File.Exists(physicalPath))
			{
				return false;
			}

			File.Delete(physicalPath);

			return true;
		}

		public bool DeleteProductDirectory(int productId)
		{
			string directory = Path.Combine(
				_environment.WebRootPath,
				"images",
				"products",
				productId.ToString());

			if (!Directory.Exists(directory))
			{
				return false;
			}

			Directory.Delete(directory, recursive: true);

			return true;
		}
	}
}
