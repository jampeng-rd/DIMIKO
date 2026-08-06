using System.Security.Cryptography;

namespace ECommerce.Utility.Helpers
{
	public class OrderNumberGenerator
	{
		private const string Characters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

		public static string Generate()
		{
			var randomPart = GenerateRandomPart(6);

			return $"DMK-{DateTime.UtcNow:yyyyMMdd}-{randomPart}";
		}

		private static string GenerateRandomPart(int length)
		{
			var characters = new char[length];

			for (var index = 0; index < length; index++)
			{
				var randomIndex =
					RandomNumberGenerator.GetInt32(Characters.Length);

				characters[index] = Characters[randomIndex];
			}

			return new string(characters);
		}

	}
}
