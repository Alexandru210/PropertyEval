using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Constants;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Users;

public class GetEvaluatorsEndpoint : EndpointWithoutRequest<IReadOnlyList<UserResponse>>
{
    private readonly UserService _userService;

    public GetEvaluatorsEndpoint(UserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Get("/users/evaluators");
        Roles(SystemRoles.Admin);
        Summary(s =>
        {
            s.Summary = "List evaluator users";
            s.Description = "Returns users with the Evaluator role for admin assignment workflows.";
        });
        Description(x => x
            .WithName("GetEvaluators")
            .Produces<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var evaluators = await _userService.GetEvaluatorsAsync(ct);

        await Send.OkAsync(evaluators, ct);
    }
}
