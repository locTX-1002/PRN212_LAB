using System.Linq;

namespace BusinessObjects
{
    public enum PasswordStrength
    {
        TooShort = 0,
        Weak = 1,
        Medium = 2,
        Strong = 3
    }

    public static class PasswordPolicy
    {
        public const int MinLength = 6;

        public static PasswordStrength Evaluate(string? password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < MinLength)
                return PasswordStrength.TooShort;

            int score = 0;
            if (password.Any(char.IsUpper)) score++;
            if (password.Any(char.IsLower)) score++;
            if (password.Any(char.IsDigit)) score++;
            if (password.Any(c => !char.IsLetterOrDigit(c))) score++;
            if (password.Length >= 10) score++;

            return score switch
            {
                <= 2 => PasswordStrength.Weak,
                3 => PasswordStrength.Medium,
                _ => PasswordStrength.Strong
            };
        }

        public static string Describe(PasswordStrength strength) => strength switch
        {
            PasswordStrength.TooShort => $"Tối thiểu {MinLength} ký tự",
            PasswordStrength.Weak => "Yếu",
            PasswordStrength.Medium => "Trung bình",
            PasswordStrength.Strong => "Mạnh",
            _ => ""
        };
    }
}
