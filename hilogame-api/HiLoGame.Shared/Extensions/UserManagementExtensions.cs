using System.Security.Claims;

namespace HiLoGame.Shared.Extensions;

public static class UserManagerExtensions
{
    public static string GetId(this ClaimsPrincipal user) // Can't return null 
    {
        return user.Claims.First(s => s.Type == ClaimTypes.NameIdentifier).Value;
    }
}