using Microsoft.AspNetCore.Identity;

namespace ECommerce.Utility.Identity
{
	public class ChineseIdentityErrorDescriber : IdentityErrorDescriber
	{
		public override IdentityError DefaultError()
		{
			return new IdentityError
			{
				Code = nameof(DefaultError),
				Description = "發生未知錯誤，請稍後再試。"
			};
		}

		public override IdentityError ConcurrencyFailure()
		{
			return new IdentityError
			{
				Code = nameof(ConcurrencyFailure),
				Description = "資料已被修改，請重新操作。"
			};
		}

		public override IdentityError PasswordMismatch()
		{
			return new IdentityError
			{
				Code = nameof(PasswordMismatch),
				Description = "密碼不正確。"
			};
		}

		public override IdentityError InvalidToken()
		{
			return new IdentityError
			{
				Code = nameof(InvalidToken),
				Description = "驗證權杖無效。"
			};
		}

		public override IdentityError LoginAlreadyAssociated()
		{
			return new IdentityError
			{
				Code = nameof(LoginAlreadyAssociated),
				Description = "此登入方式已與其他帳號綁定。"
			};
		}

		public override IdentityError InvalidUserName(string? userName)
		{
			return new IdentityError
			{
				Code = nameof(InvalidUserName),
				Description = $"帳號「{userName}」格式不正確。"
			};
		}

		public override IdentityError InvalidEmail(string? email)
		{
			return new IdentityError
			{
				Code = nameof(InvalidEmail),
				Description = $"電子郵件「{email}」格式不正確。"
			};
		}

		public override IdentityError DuplicateUserName(string userName)
		{
			return new IdentityError
			{
				Code = nameof(DuplicateUserName),
				Description = $"帳號「{userName}」已被使用。"
			};
		}

		public override IdentityError DuplicateEmail(string email)
		{
			return new IdentityError
			{
				Code = nameof(DuplicateEmail),
				Description = $"電子郵件「{email}」已被註冊。"
			};
		}

		public override IdentityError InvalidRoleName(string? role)
		{
			return new IdentityError
			{
				Code = nameof(InvalidRoleName),
				Description = $"角色名稱「{role}」格式不正確。"
			};
		}

		public override IdentityError DuplicateRoleName(string role)
		{
			return new IdentityError
			{
				Code = nameof(DuplicateRoleName),
				Description = $"角色「{role}」已經存在。"
			};
		}

		public override IdentityError UserAlreadyHasPassword()
		{
			return new IdentityError
			{
				Code = nameof(UserAlreadyHasPassword),
				Description = "此使用者已經設定密碼。"
			};
		}

		public override IdentityError UserLockoutNotEnabled()
		{
			return new IdentityError
			{
				Code = nameof(UserLockoutNotEnabled),
				Description = "此使用者未啟用帳號鎖定功能。"
			};
		}

		public override IdentityError UserAlreadyInRole(string role)
		{
			return new IdentityError
			{
				Code = nameof(UserAlreadyInRole),
				Description = $"使用者已經擁有「{role}」角色。"
			};
		}

		public override IdentityError UserNotInRole(string role)
		{
			return new IdentityError
			{
				Code = nameof(UserNotInRole),
				Description = $"使用者不屬於「{role}」角色。"
			};
		}

		public override IdentityError PasswordTooShort(int length)
		{
			return new IdentityError
			{
				Code = nameof(PasswordTooShort),
				Description = $"密碼長度至少需要 {length} 個字元。"
			};
		}

		public override IdentityError PasswordRequiresNonAlphanumeric()
		{
			return new IdentityError
			{
				Code = nameof(PasswordRequiresNonAlphanumeric),
				Description = "密碼至少需要包含一個特殊符號，例如 !、@、#、$。"
			};
		}

		public override IdentityError PasswordRequiresDigit()
		{
			return new IdentityError
			{
				Code = nameof(PasswordRequiresDigit),
				Description = "密碼至少需要包含一個數字（0-9）。"
			};
		}

		public override IdentityError PasswordRequiresLower()
		{
			return new IdentityError
			{
				Code = nameof(PasswordRequiresLower),
				Description = "密碼至少需要包含一個英文小寫字母（a-z）。"
			};
		}

		public override IdentityError PasswordRequiresUpper()
		{
			return new IdentityError
			{
				Code = nameof(PasswordRequiresUpper),
				Description = "密碼至少需要包含一個英文大寫字母（A-Z）。"
			};
		}

		public override IdentityError PasswordRequiresUniqueChars(int uniqueChars)
		{
			return new IdentityError
			{
				Code = nameof(PasswordRequiresUniqueChars),
				Description = $"密碼至少需要包含 {uniqueChars} 個不同的字元。"
			};
		}

		public override IdentityError RecoveryCodeRedemptionFailed()
		{
			return new IdentityError
			{
				Code = nameof(RecoveryCodeRedemptionFailed),
				Description = "備援碼驗證失敗。"
			};
		}

	}
}
