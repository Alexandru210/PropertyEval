using Microsoft.EntityFrameworkCore;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Entities;
using PropertyEval.Infrastructure.Data;

namespace PropertyEval.Infrastructure.Services;

public class ListingService
{
    private readonly AppDbContext _context;

    public ListingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ListingResponse> CreateListingAsync(CreateListingRequest request, int userId, CancellationToken cancellationToken)
    {
        var property = await _context.Properties
            .Include(p => p.Address)
            .SingleOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken);

        if (property is null)
        {
            throw new KeyNotFoundException("Property was not found.");
        }

        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException("User was not found.");
        }

        var now = DateTime.UtcNow;
        var listing = new Listing
        {
            PropertyId = property.Id,
            Property = property,
            UserId = user.Id,
            User = user,
            Title = request.Title.Trim(),
            AskingPrice = request.AskingPrice,
            Status = request.Status,
            CreatedAt = now
        };

        _context.Listings.Add(listing);
        await _context.SaveChangesAsync(cancellationToken);

        return ResponseMapper.ToResponse(listing);
    }

    public async Task<ListingResponse> GetListingAsync(int id, CancellationToken cancellationToken)
    {
        var listing = await CreateListingQuery()
            .SingleOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (listing is null)
        {
            throw new KeyNotFoundException("Listing was not found.");
        }

        return ResponseMapper.ToResponse(listing);
    }

    public async Task<IReadOnlyList<ListingResponse>> GetListingsAsync(GetListingsRequest request, CancellationToken cancellationToken)
    {
        var query = CreateListingQuery();

        if (request.PropertyId is not null)
        {
            query = query.Where(l => l.PropertyId == request.PropertyId);
        }

        if (request.UserId is not null)
        {
            query = query.Where(l => l.UserId == request.UserId);
        }

        if (request.Status is not null)
        {
            query = query.Where(l => l.Status == request.Status);
        }

        if (request.MinAskingPrice is not null)
        {
            query = query.Where(l => l.AskingPrice >= request.MinAskingPrice);
        }

        if (request.MaxAskingPrice is not null)
        {
            query = query.Where(l => l.AskingPrice <= request.MaxAskingPrice);
        }

        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var listings = await query
            .OrderByDescending(l => l.CreatedAt)
            .ThenByDescending(l => l.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return listings.Select(ResponseMapper.ToResponse).ToList();
    }

    private IQueryable<Listing> CreateListingQuery()
    {
        return _context.Listings
            .Include(l => l.User)
            .Include(l => l.Property)
            .ThenInclude(p => p.Address)
            .AsNoTracking();
    }
}
