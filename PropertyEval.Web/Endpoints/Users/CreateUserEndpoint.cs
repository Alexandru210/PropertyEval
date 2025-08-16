using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Users
{
    public class CreateUserEndpoint : Endpoint<CreateUserRequest, UserResponse>
    {
        private readonly UserService _userService;

        public CreateUserEndpoint(UserService userService)
        {
            _userService = userService;
        }
        public override void Configure()
        {
            Post("/users");
            AllowAnonymous();
            Description(x => x
                .WithName("CreateUser")
                .Produces<UserResponse>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status409Conflict)
                );
        }
        public override async Task HandleAsync(CreateUserRequest request, CancellationToken ct)
        {
            try
            {
                var result = await _userService.CreateUserAsync(request, ct);

                var response = new UserResponse(
                    result.Id,
                    result.FirstName,
                    result.LastName,
                    result.Email
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
}
