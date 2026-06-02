using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Properties;

public class GetPropertyEndpoint : Endpoint<GetPropertyRequest, PropertyResponse>
{
    private readonly PropertyService _propertyService;

    public GetPropertyEndpoint(PropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    public override void Configure()
    {
        Get("/properties/{id}");
        AllowAnonymous();
        Description(x => x
            .WithName("GetProperty")
            .Produces<PropertyResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(GetPropertyRequest request, CancellationToken ct)
    {
        try
        {
            var property = await _propertyService.GetPropertyAsync(request.Id, ct);

            await Send.OkAsync(property, ct);
        }
        catch (KeyNotFoundException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(StatusCodes.Status404NotFound, ct);
        }
    }
}
