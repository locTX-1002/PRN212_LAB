namespace BusinessObjects;

/// <summary>
/// Hash & verify password with BCrypt.
/// Hỗ trợ fallback so sánh plain-text cho dữ liệu cũ chưa migrate
/// (AccountMember.MemberPassword nhập tay từ MyStoreDB.sql).
/// </summary>
public static class PasswordHasher
{
    public static string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    public static bool Verify(string password, string? stored)
    {
        if (string.IsNullOrEmpty(stored)) return false;

        // BCrypt hash bắt đầu bằng $2a$, $2b$, $2y$...
        if (stored.StartsWith("$2"))
        {
            try { return BCrypt.Net.BCrypt.Verify(password, stored); }
            catch { return false; }
        }

        // Legacy plain-text fallback (lab dùng INSERT thẳng vào DB)
        return stored == password;
    }
}
