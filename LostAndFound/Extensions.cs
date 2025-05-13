using System.Security.Cryptography;
using System.Text;

namespace LostAndFound;

public static class Extensions
{
    public static string HashPassword(this string password)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        var hash = Convert.ToHexStringLower(hashBytes);
        return hash;
    }
}