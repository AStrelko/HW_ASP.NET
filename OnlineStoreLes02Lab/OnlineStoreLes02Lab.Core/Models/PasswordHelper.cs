using System.Security.Cryptography;
using System.Text;

namespace OnlineStoreLes02Lab.Core.Models;

public static class PasswordHelper//клас для перетвореня пароля в hash рядок
{
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}