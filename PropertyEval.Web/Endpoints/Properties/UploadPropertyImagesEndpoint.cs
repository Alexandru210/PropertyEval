using FastEndpoints;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Common;
using PropertyEval.Domain.Constants;
using PropertyEval.Infrastructure.Services;

namespace PropertyEval.Web.Endpoints.Properties;

public class UploadPropertyImagesRequest
{
    public int PropertyId { get; set; }
}

public class UploadPropertyImagesEndpoint : Endpoint<UploadPropertyImagesRequest, IReadOnlyList<PropertyImageResponse>>
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private readonly PropertyImageService _propertyImageService;

    public UploadPropertyImagesEndpoint(PropertyImageService propertyImageService)
    {
        _propertyImageService = propertyImageService;
    }

    public override void Configure()
    {
        Post("/properties/{propertyId}/images");
        Roles(SystemRoles.Client, SystemRoles.Admin);
        AllowFileUploads();
        Summary(s =>
        {
            s.Summary = "Upload property images";
            s.Description = "Stores property images locally under wwwroot and links them to an owned property.";
        });
        Description(x => x
            .WithName("UploadPropertyImages")
            .Accepts<IFormFileCollection>("multipart/form-data")
            .Produces<IReadOnlyList<PropertyImageResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(UploadPropertyImagesRequest request, CancellationToken ct)
    {
        var files = Files.ToList();

        if (!ValidateFiles(files))
        {
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
            return;
        }

        var uploads = new List<PropertyImageUpload>();

        try
        {
            uploads.AddRange(files.Select(file => new PropertyImageUpload(
                file.OpenReadStream(),
                file.FileName,
                file.ContentType,
                file.Length,
                null)));

            var userId = User.GetRequiredUserId();
            var images = await _propertyImageService.UploadImagesAsync(
                request.PropertyId,
                userId,
                User.IsInRole(SystemRoles.Admin),
                uploads,
                ct);

            await Send.OkAsync(images, ct);
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
        catch (InvalidOperationException ex)
        {
            AddError(ex.Message);
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
        }
        finally
        {
            foreach (var upload in uploads)
            {
                await upload.Content.DisposeAsync();
            }
        }
    }

    private bool ValidateFiles(IReadOnlyList<IFormFile> files)
    {
        if (files.Count == 0)
        {
            AddError("At least one image is required.");
            return false;
        }

        if (files.Count > PropertyImageService.MaxImagesPerProperty)
        {
            AddError($"A property can have up to {PropertyImageService.MaxImagesPerProperty} images.");
            return false;
        }

        foreach (var file in files)
        {
            if (file.Length == 0)
            {
                AddError($"Image '{file.FileName}' is empty.");
                return false;
            }

            if (file.Length > MaxFileSizeBytes)
            {
                AddError($"Image '{file.FileName}' exceeds the 5 MB limit.");
                return false;
            }

            if (!AllowedContentTypes.Contains(file.ContentType))
            {
                AddError($"Image '{file.FileName}' must be a JPG, PNG, or WebP file.");
                return false;
            }
        }

        return true;
    }
}
