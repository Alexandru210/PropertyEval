using FastEndpoints;
using FastEndpoints.Security;
using PropertyEval.Application.DTOs;
using PropertyEval.Infrastructure.Services;
using System.Security.Claims;

namespace PropertyEval.Web.Endpoints.Users;

public class CreateUserEndpoint : Endpoint<CreateUserRequest, CreateUserResponse>
{
    private readonly UserService _userService;
    private readonly int _tokenExpirationMinutes;

    public CreateUserEndpoint(UserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _tokenExpirationMinutes = int.TryParse(configuration["Jwt:TokenExpirationMinutes"], out var minutes)
            ? minutes
            : 60;
    }

    public override void Configure()
    {
        Post("/users");
        AllowAnonymous();
        Description(x => x
            .WithName("CreateUser")
            .Produces<CreateUserResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            );
    }
    public override async Task HandleAsync(CreateUserRequest request, CancellationToken ct)
    {
        try
        {
            var user = await _userService.CreateUserAsync(request, ct);

            var token = JwtBearer.CreateToken(options =>
            {
                options.ExpireAt = DateTime.UtcNow.AddMinutes(_tokenExpirationMinutes);
                options.User.Claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                options.User.Claims.Add(new Claim(ClaimTypes.Email, user.Email));
                options.User.Claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
                options.User.Claims.Add(new Claim(ClaimTypes.Surname, user.LastName));
            });

            var response = new CreateUserResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                token
            );

            await Send.CreatedAtAsync<CreateUserEndpoint>(responseBody: response, cancellation: ct);
        }
        catch (InvalidOperationException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(StatusCodes.Status409Conflict, ct);
        }
    }
}
