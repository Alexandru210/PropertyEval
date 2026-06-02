using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PropertyEval.Web.Endpoints;

internal static class EndpointUserExtensions
{
    public static int GetRequiredUserId(this ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue("sub");

        if (!int.TryParse(userId, out var parsedUserId))
        {
            throw new UnauthorizedAccessException("Authenticated user id is missing.");
        }

        return parsedUserId;
    }
}
