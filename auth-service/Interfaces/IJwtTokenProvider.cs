using System.Security.Claims;
using QuickBite.Auth.Entities;

namespace QuickBite.Auth.Interfaces
{
    public interface IJwtTokenProvider
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
        int? GetUserIdFromToken(string token);
    }
}
