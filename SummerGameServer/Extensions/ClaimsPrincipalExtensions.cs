using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SummerGameServer.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool TryGetUserId(this ClaimsPrincipal principal, out int userId)
        {
            var value = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue("sub");

            return int.TryParse(value, out userId);
        }
    }
}
