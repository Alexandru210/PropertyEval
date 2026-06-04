using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Constants;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Evaluations;

public class AssignEvaluationEndpoint : Endpoint<AssignEvaluationRequest, EvaluationResponse>
{
    private readonly EvaluationService _evaluationService;

    public AssignEvaluationEndpoint(EvaluationService evaluationService)
    {
        _evaluationService = evaluationService;
    }

    public override void Configure()
    {
        Post("/evaluations/{id}/assign");
        Roles(SystemRoles.Admin);
        Description(x => x
            .WithName("AssignEvaluation")
            .Produces<EvaluationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(AssignEvaluationRequest request, CancellationToken ct)
    {
        try
        {
            var evaluation = await _evaluationService.AssignEvaluationAsync(
                request.Id,
                request.EvaluatorUserId,
                ct);

            await Send.OkAsync(evaluation, ct);
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
