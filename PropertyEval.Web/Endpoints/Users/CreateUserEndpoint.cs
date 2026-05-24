using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Entities;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Users;

public class CreateUserEndpoint : Endpoint<CreateUserRequest, CreateUserResponse>
{
    private readonly UserService _userService;
    private readonly AuthenticationService _authenticationService;

    public CreateUserEndpoint(UserService userService, AuthenticationService authenticationService)
    {
        _userService = userService;
        _authenticationService = authenticationService;
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

            // Generate JWT token
            var token = _authenticationService.GenerateToken(new User
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
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
