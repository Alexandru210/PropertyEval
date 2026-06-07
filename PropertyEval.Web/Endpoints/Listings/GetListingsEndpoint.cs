using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Constants;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Listings;

public class GetListingsEndpoint : Endpoint<GetListingsRequest, IReadOnlyList<ListingResponse>>
{
    private readonly ListingService _listingService;

    public GetListingsEndpoint(ListingService listingService)
    {
        _listingService = listingService;
    }

    public override void Configure()
    {
        Get("/listings");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Search listings";
            s.Description = "Returns paged listings filtered by property, seller, status, and asking price within the caller's access.";
        });
        Description(x => x
            .WithName("GetListings")
            .Produces<IReadOnlyList<ListingResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest));
    }

    public override async Task HandleAsync(GetListingsRequest request, CancellationToken ct)
    {
        int? currentUserId = User.Identity?.IsAuthenticated == true
            ? User.GetRequiredUserId()
            : null;
        var listings = await _listingService.GetListingsAsync(
            request,
            currentUserId,
            User.IsInRole(SystemRoles.Admin),
            ct);

        await Send.OkAsync(listings, ct);
    }
}
