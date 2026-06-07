using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Constants;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Evaluations;

public class GetEvaluationsEndpoint : Endpoint<GetEvaluationsRequest, IReadOnlyList<EvaluationResponse>>
{
    private readonly EvaluationService _evaluationService;

    public GetEvaluationsEndpoint(EvaluationService evaluationService)
    {
        _evaluationService = evaluationService;
    }

    public override void Configure()
    {
        Get("/evaluations");
        Roles(SystemRoles.Client, SystemRoles.Evaluator, SystemRoles.Admin);
        Summary(s =>
        {
            s.Summary = "Search evaluations";
            s.Description = "Returns paged evaluation requests filtered by property, user, evaluator, status, and date range.";
        });
        Description(x => x
            .WithName("GetEvaluations")
            .Produces<IReadOnlyList<EvaluationResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden));
    }

    public override async Task HandleAsync(GetEvaluationsRequest request, CancellationToken ct)
    {
        try
        {
            var userId = User.GetRequiredUserId();
            var canViewAllEvaluations = User.IsInRole(SystemRoles.Admin);
            var canViewAssignedEvaluations = User.IsInRole(SystemRoles.Evaluator);
            var evaluations = await _evaluationService.GetEvaluationsAsync(
                request,
                userId,
                canViewAllEvaluations,
                canViewAssignedEvaluations,
                ct);

            await Send.OkAsync(evaluations, ct);
        }
        catch (UnauthorizedAccessException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(StatusCodes.Status401Unauthorized, ct);
        }
    }
}
