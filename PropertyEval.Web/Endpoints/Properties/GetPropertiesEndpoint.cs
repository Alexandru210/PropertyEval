using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Properties;

public class GetPropertiesEndpoint : Endpoint<GetPropertiesRequest, IReadOnlyList<PropertyResponse>>
{
    private readonly PropertyService _propertyService;

    public GetPropertiesEndpoint(PropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    public override void Configure()
    {
        Get("/properties");
        AllowAnonymous();
        Description(x => x
            .WithName("GetProperties")
            .Produces<IReadOnlyList<PropertyResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest));
    }

    public override async Task HandleAsync(GetPropertiesRequest request, CancellationToken ct)
    {
        var properties = await _propertyService.GetPropertiesAsync(request, ct);

        await Send.OkAsync(properties, ct);
    }
}
