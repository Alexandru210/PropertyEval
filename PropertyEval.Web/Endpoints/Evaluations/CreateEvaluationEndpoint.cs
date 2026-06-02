using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Constants;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Evaluations;

public class CreateEvaluationEndpoint : Endpoint<CreateEvaluationRequest, EvaluationResponse>
{
    private readonly EvaluationService _evaluationService;

    public CreateEvaluationEndpoint(EvaluationService evaluationService)
    {
        _evaluationService = evaluationService;
    }

    public override void Configure()
    {
        Post("/evaluations");
        Roles(SystemRoles.Client, SystemRoles.Admin);
        Description(x => x
            .WithName("CreateEvaluation")
            .Produces<EvaluationResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(CreateEvaluationRequest request, CancellationToken ct)
    {
        try
        {
            var userId = User.GetRequiredUserId();
            var evaluation = await _evaluationService.CreateEvaluationAsync(request, userId, ct);

            await Send.CreatedAtAsync(
                "GetEvaluation",
                new { id = evaluation.Id },
                evaluation,
                generateAbsoluteUrl: false,
                cancellation: ct);
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
