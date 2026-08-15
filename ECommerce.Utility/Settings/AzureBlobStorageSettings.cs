namespace ECommerce.Utility.Settings
{
	public class AzureBlobStorageSettings
	{
		public const string SectionName = "AzureBlobStorage";

		public string AccountName { get; set; } = string.Empty;

		public string ContainerName { get; set; } = string.Empty;
	}
}
