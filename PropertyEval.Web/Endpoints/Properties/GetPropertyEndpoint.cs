using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Constants;
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
        Summary(s =>
        {
            s.Summary = "Get property details";
            s.Description = "Returns a public active-listing property, or an owner/admin-visible property when authenticated.";
        });
        Description(x => x
            .WithName("GetProperty")
            .Produces<PropertyResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(GetPropertyRequest request, CancellationToken ct)
    {
        try
        {
            int? currentUserId = User.Identity?.IsAuthenticated == true
                ? User.GetRequiredUserId()
                : null;
            var property = await _propertyService.GetPropertyAsync(
                request.Id,
                currentUserId,
                User.IsInRole(SystemRoles.Admin),
                ct);

            await Send.OkAsync(property, ct);
        }
        catch (KeyNotFoundException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(StatusCodes.Status404NotFound, ct);
        }
    }
}
