using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Constants;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Listings;

public class GetListingEndpoint : Endpoint<GetListingRequest, ListingResponse>
{
    private readonly ListingService _listingService;

    public GetListingEndpoint(ListingService listingService)
    {
        _listingService = listingService;
    }

    public override void Configure()
    {
        Get("/listings/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get listing details";
            s.Description = "Returns active listings publicly and also returns the current user's own inactive listings.";
        });
        Description(x => x
            .WithName("GetListing")
            .Produces<ListingResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(GetListingRequest request, CancellationToken ct)
    {
        try
        {
            int? currentUserId = User.Identity?.IsAuthenticated == true
                ? User.GetRequiredUserId()
                : null;
            var listing = await _listingService.GetListingAsync(
                request.Id,
                currentUserId,
                User.IsInRole(SystemRoles.Admin),
                ct);

            await Send.OkAsync(listing, ct);
        }
        catch (KeyNotFoundException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(StatusCodes.Status404NotFound, ct);
        }
    }
}
