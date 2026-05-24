using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PropertyEval.Domain.Entities;

namespace PropertyEval.Infrastructure.Services;

public class AuthenticationService
{
    private readonly string _jwtSecret;
    private readonly int _tokenExpirationMinutes;

    public AuthenticationService(string jwtSecret, int tokenExpirationMinutes = 60)
    {
        _jwtSecret = jwtSecret;
        _tokenExpirationMinutes = tokenExpirationMinutes;
    }

    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
            }),
            Expires = DateTime.UtcNow.AddMinutes(_tokenExpirationMinutes),
            Issuer = "PropertyEval",
            Audience = "PropertyEval",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
