using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Constants;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Users;

public class UpdateUserRoleEndpoint : Endpoint<UpdateUserRoleRequest, UserResponse>
{
    private readonly UserService _userService;

    public UpdateUserRoleEndpoint(UserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Patch("/users/{id}/role");
        Roles(SystemRoles.Admin);
        Description(x => x
            .WithName("UpdateUserRole")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(UpdateUserRoleRequest request, CancellationToken ct)
    {
        try
        {
            var user = await _userService.UpdateUserRoleAsync(request.Id, request.Role, ct);

            await Send.OkAsync(user, ct);
        }
        catch (KeyNotFoundException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(StatusCodes.Status404NotFound, ct);
        }
        catch (InvalidOperationException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
        }
    }
}
