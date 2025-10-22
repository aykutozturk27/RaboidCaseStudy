using System.Security.Cryptography;
using System.Text;

namespace RaboidCaseStudy.Infrastructure.Security;
public static class PasswordHasher
{
    public static (string Hash, string Salt) HashPassword(string password, string? saltBase64 = null)
    {
        var salt = saltBase64 is null ? RandomNumberGenerator.GetBytes(16) : Convert.FromBase64String(saltBase64);
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }
    public static bool Verify(string password, string hashBase64, string saltBase64)
    {
        var (hash, _) = HashPassword(password, saltBase64);
        return CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(hashBase64), Convert.FromBase64String(hash));
    }
}
