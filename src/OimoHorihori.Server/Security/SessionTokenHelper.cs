using System.Security.Cryptography;
using System.Text;

namespace OimoHorihori.Server.Security;

public static class SessionTokenHelper
{
    public static string CreateToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);

        return Convert.ToHexString(bytes);
    }

    public static string HashToken(string token)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(token);
        byte[] hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}