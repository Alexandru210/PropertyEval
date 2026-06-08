using FastEndpoints;
using PropertyEval.Domain.Common;
using PropertyEval.Domain.Constants;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Properties;

public class DeletePropertyImageRequest
{
    public int PropertyId { get; set; }
    public int ImageId { get; set; }
}

public class DeletePropertyImageEndpoint : Endpoint<DeletePropertyImageRequest>
{
    private readonly PropertyImageService _propertyImageService;

    public DeletePropertyImageEndpoint(PropertyImageService propertyImageService)
    {
        _propertyImageService = propertyImageService;
    }

    public override void Configure()
    {
        Delete("/properties/{propertyId}/images/{imageId}");
        Roles(SystemRoles.Client, SystemRoles.Admin);
        Summary(s =>
        {
            s.Summary = "Delete a property image";
            s.Description = "Deletes a locally stored property image owned by the current user, or by any user for admins.";
        });
        Description(x => x
            .WithName("DeletePropertyImage")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(DeletePropertyImageRequest request, CancellationToken ct)
    {
        try
        {
            var userId = User.GetRequiredUserId();
            await _propertyImageService.DeleteImageAsync(
                request.PropertyId,
                request.ImageId,
                userId,
                User.IsInRole(SystemRoles.Admin),
                ct);

            await Send.NoContentAsync(ct);
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
