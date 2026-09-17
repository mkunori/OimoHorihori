using System.Security.Cryptography;
using System.Text;

namespace OimoHorihori.Server.Security;

public class AdminAuthService
{
    private readonly IConfiguration configuration;

    public AdminAuthService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public bool IsAuthorized(HttpRequest request)
    {
        string? expectedKey = configuration["Admin:Key"];

        if (string.IsNullOrWhiteSpace(expectedKey))
        {
            return false;
        }

        string suppliedKey = request.Headers["X-Admin-Key"].ToString();

        if (string.IsNullOrWhiteSpace(suppliedKey))
        {
            return false;
        }

        byte[] expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expectedKey));

        byte[] suppliedHash = SHA256.HashData(Encoding.UTF8.GetBytes(suppliedKey));

        return CryptographicOperations.FixedTimeEquals(expectedHash, suppliedHash);
    }
}