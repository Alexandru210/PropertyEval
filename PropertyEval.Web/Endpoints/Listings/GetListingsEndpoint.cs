using FastEndpoints;
using PropertyEval.Application.DTOs;
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
        Description(x => x
            .WithName("GetListings")
            .Produces<IReadOnlyList<ListingResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest));
    }

    public override async Task HandleAsync(GetListingsRequest request, CancellationToken ct)
    {
        var listings = await _listingService.GetListingsAsync(request, ct);

        await Send.OkAsync(listings, ct);
    }
}
