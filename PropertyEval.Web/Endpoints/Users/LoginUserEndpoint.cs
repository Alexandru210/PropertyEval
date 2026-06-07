using FastEndpoints;
using FastEndpoints.Security;
using PropertyEval.Application.DTOs;
using PropertyEval.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PropertyEval.Web.Endpoints.Users;

public class LoginUserEndpoint : Endpoint<LoginUserRequest, LoginUserResponse>
{
    private readonly UserService _userService;
    private readonly int _tokenExpirationMinutes;

    public LoginUserEndpoint(UserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _tokenExpirationMinutes = int.TryParse(configuration["Jwt:TokenExpirationMinutes"], out var minutes)
            ? minutes
            : 60;
    }

    public override void Configure()
    {
        Post("/users/login");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Sign in a user";
            s.Description = "Authenticates a user with email and password and returns a JWT bearer token.";
        });
        Description(x => x
            .WithName("LoginUser")
            .Produces<LoginUserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized));
    }

    public override async Task HandleAsync(LoginUserRequest request, CancellationToken ct)
    {
        try
        {
            var user = await _userService.AuthenticateUserAsync(request, ct);

            var token = JwtBearer.CreateToken(options =>
            {
                options.ExpireAt = DateTime.UtcNow.AddMinutes(_tokenExpirationMinutes);
                options.User.Claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()));
                options.User.Claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
                options.User.Claims.Add(new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName));
                options.User.Claims.Add(new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName));
                options.User.Claims.Add(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", user.Role));
                options.User.Claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            });

            var response = new LoginUserResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                token
            );

            await Send.OkAsync(response, ct);
        }
        catch (UnauthorizedAccessException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(StatusCodes.Status401Unauthorized, ct);
        }
    }
}
