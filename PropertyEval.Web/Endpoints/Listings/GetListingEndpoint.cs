using FastEndpoints;
using PropertyEval.Application.DTOs;
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
        Description(x => x
            .WithName("GetListing")
            .Produces<ListingResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(GetListingRequest request, CancellationToken ct)
    {
        try
        {
            var listing = await _listingService.GetListingAsync(request.Id, ct);

            await Send.OkAsync(listing, ct);
        }
        catch (KeyNotFoundException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(StatusCodes.Status404NotFound, ct);
        }
    }
}
