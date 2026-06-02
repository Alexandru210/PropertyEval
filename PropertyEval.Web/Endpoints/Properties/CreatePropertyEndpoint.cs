using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Constants;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Properties;

public class CreatePropertyEndpoint : Endpoint<CreatePropertyRequest, PropertyResponse>
{
    private readonly PropertyService _propertyService;

    public CreatePropertyEndpoint(PropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    public override void Configure()
    {
        Post("/properties");
        Roles(SystemRoles.Client, SystemRoles.Admin);
        Description(x => x
            .WithName("CreateProperty")
            .Produces<PropertyResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden));
    }

    public override async Task HandleAsync(CreatePropertyRequest request, CancellationToken ct)
    {
        var property = await _propertyService.CreatePropertyAsync(request, ct);

        await Send.CreatedAtAsync(
            "GetProperty",
            new { id = property.Id },
            property,
            generateAbsoluteUrl: false,
            cancellation: ct);
    }
}
