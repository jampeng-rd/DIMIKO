

namespace ECommerce.Utility.Helpers
{
	public static class TaiwanTimeHelper
	{
		public static TimeZoneInfo GetTaiwanTimeZone()
		{
			/*
			 * Windows 使用 Taipei Standard Time。
			 * Linux / Docker 通常使用 Asia/Taipei。
			 */
			var timeZoneIds = new[]
			{
				"Asia/Taipei",
				"Taipei Standard Time"
			};

			foreach (var timeZoneId in timeZoneIds)
			{
				try
				{
					return TimeZoneInfo.FindSystemTimeZoneById(
						timeZoneId
					);
				}
				catch (TimeZoneNotFoundException)
				{
					// 嘗試下一個時區 ID
				}
				catch (InvalidTimeZoneException)
				{
					// 嘗試下一個時區 ID
				}
			}

			throw new TimeZoneNotFoundException("找不到台灣時區設定");
		}

		public static DateTime GetTaiwanNow()
		{
			return TimeZoneInfo.ConvertTimeFromUtc(
				DateTime.UtcNow,
				GetTaiwanTimeZone()
			);
		}

		public static DateTime ConvertTaiwanToUtc(DateTime taiwanDateTime)
		{
			var unspecifiedDateTime = DateTime.SpecifyKind(taiwanDateTime, DateTimeKind.Unspecified);

			return TimeZoneInfo.ConvertTimeToUtc(
				unspecifiedDateTime,
				GetTaiwanTimeZone()
			);
		}

		public static DateTime ConvertUtcToTaiwan(DateTime utcDateTime)
		{
			var normalizedUtcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

			return TimeZoneInfo.ConvertTimeFromUtc(
				normalizedUtcDateTime,
				GetTaiwanTimeZone()
			);
		}

	}
}
