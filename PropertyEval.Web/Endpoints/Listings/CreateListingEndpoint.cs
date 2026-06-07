using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Common;
using PropertyEval.Domain.Constants;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Listings;

public class CreateListingEndpoint : Endpoint<CreateListingRequest, ListingResponse>
{
    private readonly ListingService _listingService;

    public CreateListingEndpoint(ListingService listingService)
    {
        _listingService = listingService;
    }

    public override void Configure()
    {
        Post("/listings");
        Roles(SystemRoles.Client, SystemRoles.Admin);
        Summary(s =>
        {
            s.Summary = "Create a listing";
            s.Description = "Publishes or drafts a listing for a property owned by the user, with admins allowed to use any property.";
        });
        Description(x => x
            .WithName("CreateListing")
            .Produces<ListingResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(CreateListingRequest request, CancellationToken ct)
    {
        try
        {
            var userId = User.GetRequiredUserId();
            var listing = await _listingService.CreateListingAsync(
                request,
                userId,
                User.IsInRole(SystemRoles.Admin),
                ct);

            await Send.CreatedAtAsync(
                "GetListing",
                new { id = listing.Id },
                listing,
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
        catch (ForbiddenAccessException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(StatusCodes.Status403Forbidden, ct);
        }
    }
}
