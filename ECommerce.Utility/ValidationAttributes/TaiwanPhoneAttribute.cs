using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ECommerce.Utility.ValidationAttributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public sealed class TaiwanPhoneAttribute : ValidationAttribute
	{
		private static readonly IReadOnlyDictionary<string, int> AreaCodeRules =
			new Dictionary<string, int>
			{
				["0836"] = 7,
				["0823"] = 7,
				["089"] = 7,
				["049"] = 7,
				["037"] = 7,

				["02"] = 8,
				["03"] = 7,
				["04"] = 8,
				["05"] = 7,
				["06"] = 7,
				["07"] = 8,
				["08"] = 7
			};

		protected override ValidationResult? IsValid(
			object? value,
			ValidationContext validationContext)
		{
			// 空值交由 [Required] 處理
			if (value is null)
			{
				return ValidationResult.Success;
			}

			var phoneNumber = value.ToString()?.Trim();

			if (string.IsNullOrWhiteSpace(phoneNumber))
			{
				return ValidationResult.Success;
			}

			// 只允許數字、空白與連字號
			if (!Regex.IsMatch(phoneNumber, @"^[0-9\s-]+$"))
			{
				return CreateError(
					"電話號碼只能包含數字、空白或連字號"
				);
			}

			// 移除空白與連字號後再驗證
			var normalizedPhone = Regex.Replace(
				phoneNumber,
				@"[\s-]",
				string.Empty
			);

			// 台灣手機：09 開頭，共 10 碼
			if (Regex.IsMatch(normalizedPhone, @"^09\d{8}$"))
			{
				return ValidationResult.Success;
			}

			// 長區碼必須先判斷，避免 0836 被當成 08
			foreach (var rule in AreaCodeRules.OrderByDescending(rule => rule.Key.Length))
			{
				var areaCode = rule.Key;
				var localNumberLength = rule.Value;

				if (!normalizedPhone.StartsWith(
					areaCode,
					StringComparison.Ordinal))
				{
					continue;
				}

				var localNumber = normalizedPhone[areaCode.Length..];

				if (localNumber.Length != localNumberLength)
				{
					return CreateError($"區碼 {areaCode} 後方必須為 {localNumberLength} 碼");
				}

				if (!localNumber.All(char.IsDigit))
				{
					return CreateError("電話號碼格式不正確");
				}

				return ValidationResult.Success;
			}

			return CreateError(ErrorMessage ?? "請輸入有效的手機或市內電話"
			);
		}

		private ValidationResult CreateError(string message)
		{
			return new ValidationResult(message);
		}

	}
}
