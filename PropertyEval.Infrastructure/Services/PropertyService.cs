using Microsoft.EntityFrameworkCore;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Entities;
using PropertyEval.Domain.Enums;
using PropertyEval.Infrastructure.Data;

namespace PropertyEval.Infrastructure.Services;

public class PropertyService
{
    private readonly AppDbContext _context;

    public PropertyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PropertyResponse> CreatePropertyAsync(
        CreatePropertyRequest request,
        int ownerUserId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var address = new Address
        {
            Street = request.Address.Street.Trim(),
            City = request.Address.City.Trim(),
            County = request.Address.County.Trim(),
            CreatedAt = now
        };

        var property = new Property
        {
            OwnerUserId = ownerUserId,
            Address = address,
            PropertyType = request.PropertyType,
            Area = request.Area,
            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms,
            YearBuilt = request.YearBuilt,
            Description = request.Description.Trim(),
            CreatedAt = now
        };

        _context.Properties.Add(property);
        await _context.SaveChangesAsync(cancellationToken);

        return ResponseMapper.ToResponse(property);
    }

    public async Task<PropertyResponse> GetPropertyAsync(
        int id,
        int? currentUserId,
        bool canViewAllProperties,
        CancellationToken cancellationToken)
    {
        var property = await _context.Properties
            .Include(p => p.Address)
            .Include(p => p.Images)
            .AsNoTracking()
            .SingleOrDefaultAsync(
                p => p.Id == id
                    && (canViewAllProperties
                        || p.Listings.Any(l => l.Status == ListingStatus.Active)
                        || (currentUserId.HasValue && p.OwnerUserId == currentUserId.Value)),
                cancellationToken);

        if (property is null)
        {
            throw new KeyNotFoundException("Property was not found.");
        }

        return ResponseMapper.ToResponse(property);
    }

    public async Task<IReadOnlyList<PropertyResponse>> GetPropertiesAsync(GetPropertiesRequest request, CancellationToken cancellationToken)
    {
        var query = _context.Properties
            .Include(p => p.Address)
            .Include(p => p.Images)
            .Where(p => p.Listings.Any(l => l.Status == ListingStatus.Active))
            .AsNoTracking();

        if (request.PropertyType is not null)
        {
            query = query.Where(p => p.PropertyType == request.PropertyType);
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            var city = request.City.Trim();
            query = query.Where(p => p.Address.City.Contains(city));
        }

        if (!string.IsNullOrWhiteSpace(request.County))
        {
            var county = request.County.Trim();
            query = query.Where(p => p.Address.County.Contains(county));
        }

        if (request.MinArea is not null)
        {
            query = query.Where(p => p.Area >= request.MinArea);
        }

        if (request.MaxArea is not null)
        {
            query = query.Where(p => p.Area <= request.MaxArea);
        }

        if (request.MinBedrooms is not null)
        {
            query = query.Where(p => p.Bedrooms >= request.MinBedrooms);
        }

        if (request.MinBathrooms is not null)
        {
            query = query.Where(p => p.Bathrooms >= request.MinBathrooms);
        }

        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var properties = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return properties.Select(ResponseMapper.ToResponse).ToList();
    }
}
