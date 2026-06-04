using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Constants;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Properties;

public class GetPropertyValuationEndpoint : Endpoint<GetPropertyValuationRequest, PropertyValuationResponse>
{
    private readonly PropertyValuationService _valuationService;

    public GetPropertyValuationEndpoint(PropertyValuationService valuationService)
    {
        _valuationService = valuationService;
    }

    public override void Configure()
    {
        Get("/properties/{id}/valuation");
        Roles(SystemRoles.Client, SystemRoles.Admin);
        Description(x => x
            .WithName("GetPropertyValuation")
            .Produces<PropertyValuationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(GetPropertyValuationRequest request, CancellationToken ct)
    {
        try
        {
            var valuation = await _valuationService.PredictPropertyValueAsync(request.Id, ct);

            await Send.OkAsync(valuation, ct);
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
