using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Constants;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Evaluations;

public class GetEvaluationEndpoint : Endpoint<GetEvaluationRequest, EvaluationResponse>
{
    private readonly EvaluationService _evaluationService;

    public GetEvaluationEndpoint(EvaluationService evaluationService)
    {
        _evaluationService = evaluationService;
    }

    public override void Configure()
    {
        Get("/evaluations/{id}");
        Roles(SystemRoles.Client, SystemRoles.Evaluator, SystemRoles.Admin);
        Summary(s =>
        {
            s.Summary = "Get evaluation details";
            s.Description = "Returns one evaluation visible to the requester, assigned evaluator, or an administrator.";
        });
        Description(x => x
            .WithName("GetEvaluation")
            .Produces<EvaluationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(GetEvaluationRequest request, CancellationToken ct)
    {
        try
        {
            var userId = User.GetRequiredUserId();
            var canViewAllEvaluations = User.IsInRole(SystemRoles.Admin);
            var canViewAssignedEvaluations = User.IsInRole(SystemRoles.Evaluator);
            var evaluation = await _evaluationService.GetEvaluationAsync(
                request.Id,
                userId,
                canViewAllEvaluations,
                canViewAssignedEvaluations,
                ct);

            await Send.OkAsync(evaluation, ct);
        }
        catch (UnauthorizedAccessException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(StatusCodes.Status401Unauthorized, ct);
        }
        catch (KeyNotFoundException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(StatusCodes.Status404NotFound, ct);
        }
    }
}
