using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ECommerce.Business.Services.IServices;
using ECommerce.Utility.Settings;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Business.Services
{
	public class AzureBlobStorageService : IImageStorageService
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

		private readonly BlobContainerClient _containerClient;


		public AzureBlobStorageService(AzureBlobStorageSettings settings)
		{
			if (string.IsNullOrWhiteSpace(settings.AccountName))
			{
				throw new InvalidOperationException("Azure Blob Storage AccountName 尚未設定。");
			}

			if (string.IsNullOrWhiteSpace(settings.ContainerName))
			{
				throw new InvalidOperationException("Azure Blob Storage ContainerName 尚未設定。");
			}

			var serviceUri = new Uri(
				$"https://{settings.AccountName}.blob.core.windows.net");

			var blobServiceClient = new BlobServiceClient(
				serviceUri,
				new DefaultAzureCredential());

			_containerClient = blobServiceClient.GetBlobContainerClient(settings.ContainerName);
		}


		public async Task<SavedImage> SaveImageAsync(string folder, IFormFile image)
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
				.Trim('/');

			string blobName = $"{normalizedFolder}/{generatedFileName}";

			var blobClient = _containerClient.GetBlobClient(blobName);

			await using Stream stream = image.OpenReadStream();

			await blobClient.UploadAsync(
				stream,
				new BlobUploadOptions
				{
					HttpHeaders = new BlobHttpHeaders
					{
						ContentType = image.ContentType
					}
				});

			return new SavedImage
			{
				FileName = generatedFileName,
				ImageUrl = blobClient.Uri.AbsoluteUri
			};
		}


		public async Task<bool> DeleteImageAsync(string imageUrl)
		{
			if (string.IsNullOrWhiteSpace(imageUrl))
			{
				return false;
			}

			if (!Uri.TryCreate(
				imageUrl,
				UriKind.Absolute,
				out Uri? imageUri))
			{
				return false;
			}

			string containerPath = $"/{_containerClient.Name}/";

			int containerIndex = imageUri.AbsolutePath.IndexOf(
				containerPath,
				StringComparison.OrdinalIgnoreCase);

			if (containerIndex < 0)
			{
				return false;
			}

			string blobName = imageUri.AbsolutePath[
				(containerIndex + containerPath.Length)..];

			blobName = Uri.UnescapeDataString(blobName);

			var blobClient = _containerClient.GetBlobClient(blobName);

			var response =
				await blobClient.DeleteIfExistsAsync(
					DeleteSnapshotsOption.IncludeSnapshots);

			return response.Value;
		}


		public async Task<int> DeleteFolderAsync(string folder)
		{
			string normalizedFolder = folder
				.Trim()
				.Trim('/');

			string prefix = $"{normalizedFolder}/";

			int deletedCount = 0;

			var options = new GetBlobsOptions
			{
				Prefix = prefix
			};

			await foreach (BlobItem blobItem in _containerClient.GetBlobsAsync(options))
			{
				var blobClient = _containerClient.GetBlobClient(blobItem.Name);

				var response = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

				if (response.Value)
				{
					deletedCount++;
				}
			}

			return deletedCount;
		}

	}
}
