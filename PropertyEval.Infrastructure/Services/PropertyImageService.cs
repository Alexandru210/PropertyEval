using Microsoft.EntityFrameworkCore;
using PropertyEval.Application.DTOs;
using PropertyEval.Application.Interfaces;
using PropertyEval.Domain.Common;
using PropertyEval.Domain.Entities;
using PropertyEval.Infrastructure.Data;

namespace PropertyEval.Infrastructure.Services;

public class PropertyImageService
{
    public const int MaxImagesPerProperty = 10;

    private readonly AppDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public PropertyImageService(AppDbContext context, IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    public async Task<IReadOnlyList<PropertyImageResponse>> UploadImagesAsync(
        int propertyId,
        int userId,
        bool canManageAnyProperty,
        IReadOnlyList<PropertyImageUpload> uploads,
        CancellationToken cancellationToken)
    {
        if (uploads.Count == 0)
        {
            throw new InvalidOperationException("At least one image is required.");
        }

        var property = await _context.Properties
            .Include(p => p.Images)
            .SingleOrDefaultAsync(p => p.Id == propertyId, cancellationToken);

        if (property is null)
        {
            throw new KeyNotFoundException("Property was not found.");
        }

        if (!canManageAnyProperty && property.OwnerUserId != userId)
        {
            throw new ForbiddenAccessException("You can only upload images for properties you created.");
        }

        if (property.Images.Count + uploads.Count > MaxImagesPerProperty)
        {
            throw new InvalidOperationException($"A property can have up to {MaxImagesPerProperty} images.");
        }

        var uploadedUrls = new List<string>();
        var images = new List<PropertyImage>();

        try
        {
            var now = DateTime.UtcNow;

            foreach (var upload in uploads)
            {
                var imageUrl = await _fileStorageService.UploadFileAsync(
                    upload.Content,
                    $"uploads/properties/{propertyId}/{upload.FileName}",
                    upload.ContentType,
                    cancellationToken);

                uploadedUrls.Add(imageUrl);
                images.Add(new PropertyImage
                {
                    PropertyId = propertyId,
                    ImageUrl = imageUrl,
                    Description = string.IsNullOrWhiteSpace(upload.Description)
                        ? null
                        : upload.Description.Trim(),
                    UploadedAt = now
                });
            }

            _context.PropertyImages.AddRange(images);
            await _context.SaveChangesAsync(cancellationToken);

            return images.Select(ResponseMapper.ToResponse).ToList();
        }
        catch
        {
            foreach (var uploadedUrl in uploadedUrls)
            {
                await _fileStorageService.DeleteFileAsync(uploadedUrl, cancellationToken);
            }

            throw;
        }
    }

    public async Task DeleteImageAsync(
        int propertyId,
        int imageId,
        int userId,
        bool canManageAnyProperty,
        CancellationToken cancellationToken)
    {
        var image = await _context.PropertyImages
            .Include(i => i.Property)
            .SingleOrDefaultAsync(i => i.Id == imageId && i.PropertyId == propertyId, cancellationToken);

        if (image is null)
        {
            throw new KeyNotFoundException("Property image was not found.");
        }

        if (!canManageAnyProperty && image.Property.OwnerUserId != userId)
        {
            throw new ForbiddenAccessException("You can only delete images for properties you created.");
        }

        _context.PropertyImages.Remove(image);
        await _context.SaveChangesAsync(cancellationToken);
        await _fileStorageService.DeleteFileAsync(image.ImageUrl, cancellationToken);
    }
}
