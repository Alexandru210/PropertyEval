using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Constants;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Evaluations;

public class CompleteEvaluationEndpoint : Endpoint<CompleteEvaluationRequest, EvaluationResponse>
{
    private readonly EvaluationService _evaluationService;

    public CompleteEvaluationEndpoint(EvaluationService evaluationService)
    {
        _evaluationService = evaluationService;
    }

    public override void Configure()
    {
        Patch("/evaluations/{id}");
        Roles(SystemRoles.Evaluator, SystemRoles.Admin);
        Summary(s =>
        {
            s.Summary = "Complete an evaluation";
            s.Description = "Records the final evaluated value and notes for an assigned evaluation request.";
        });
        Description(x => x
            .WithName("CompleteEvaluation")
            .Produces<EvaluationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(CompleteEvaluationRequest request, CancellationToken ct)
    {
        try
        {
            var userId = User.GetRequiredUserId();
            var canCompleteAnyEvaluation = User.IsInRole(SystemRoles.Admin);
            var evaluation = await _evaluationService.CompleteEvaluationAsync(
                request,
                userId,
                canCompleteAnyEvaluation,
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
        catch (InvalidOperationException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
        }
    }
}
