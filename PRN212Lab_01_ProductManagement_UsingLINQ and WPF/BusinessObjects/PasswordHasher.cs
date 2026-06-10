namespace BusinessObjects
{
    public static class PasswordHasher
    {
        public static string Hash(string plainPassword)
            => BCrypt.Net.BCrypt.HashPassword(plainPassword);

        public static bool IsHashed(string? value)
            => !string.IsNullOrEmpty(value) && value.StartsWith("$2");

        public static bool Verify(string plainPassword, string? stored)
        {
            if (string.IsNullOrEmpty(stored)) return false;

            if (IsHashed(stored))
            {
                try { return BCrypt.Net.BCrypt.Verify(plainPassword, stored); }
                catch { return false; }
            }

            // Legacy plain-text fallback (sẽ được tự upgrade sang hash khi login thành công)
            return stored == plainPassword;
        }
    }
}
