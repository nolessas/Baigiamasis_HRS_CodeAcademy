using System.Security.Claims;

namespace Baigiamasis.Services.Auth.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(string userId, string username, List<string> roles);
        bool ValidateToken(string token);
        ClaimsPrincipal GetPrincipalFromToken(string token);
        DateTime GetTokenExpirationTime();
    }
}
