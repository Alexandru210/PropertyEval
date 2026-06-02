using Microsoft.EntityFrameworkCore;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Entities;
using PropertyEval.Infrastructure.Data;

namespace PropertyEval.Infrastructure.Services;

public class EvaluationService
{
    private readonly AppDbContext _context;

    public EvaluationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EvaluationResponse> CreateEvaluationAsync(CreateEvaluationRequest request, int userId, CancellationToken cancellationToken)
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
        var evaluation = new Evaluation
        {
            PropertyId = property.Id,
            Property = property,
            UserId = user.Id,
            User = user,
            EvaluatedValue = request.EvaluatedValue ?? 0m,
            Status = request.Status,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            EvaluationDate = request.EvaluationDate ?? now,
            CreatedAt = now
        };

        _context.Evaluations.Add(evaluation);
        await _context.SaveChangesAsync(cancellationToken);

        return ResponseMapper.ToResponse(evaluation);
    }

    public async Task<EvaluationResponse> GetEvaluationAsync(int id, int userId, bool canViewAllEvaluations, CancellationToken cancellationToken)
    {
        var query = CreateEvaluationQuery();

        if (!canViewAllEvaluations)
        {
            query = query.Where(e => e.UserId == userId);
        }

        var evaluation = await query
            .SingleOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (evaluation is null)
        {
            throw new KeyNotFoundException("Evaluation was not found.");
        }

        return ResponseMapper.ToResponse(evaluation);
    }

    public async Task<IReadOnlyList<EvaluationResponse>> GetEvaluationsAsync(
        GetEvaluationsRequest request,
        int userId,
        bool canViewAllEvaluations,
        CancellationToken cancellationToken)
    {
        var query = CreateEvaluationQuery();

        if (!canViewAllEvaluations)
        {
            query = query.Where(e => e.UserId == userId);
        }
        else if (request.UserId is not null)
        {
            query = query.Where(e => e.UserId == request.UserId);
        }

        if (request.PropertyId is not null)
        {
            query = query.Where(e => e.PropertyId == request.PropertyId);
        }

        if (request.Status is not null)
        {
            query = query.Where(e => e.Status == request.Status);
        }

        if (request.FromDate is not null)
        {
            query = query.Where(e => e.EvaluationDate >= request.FromDate);
        }

        if (request.ToDate is not null)
        {
            query = query.Where(e => e.EvaluationDate <= request.ToDate);
        }

        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var evaluations = await query
            .OrderByDescending(e => e.EvaluationDate)
            .ThenByDescending(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return evaluations.Select(ResponseMapper.ToResponse).ToList();
    }

    private IQueryable<Evaluation> CreateEvaluationQuery()
    {
        return _context.Evaluations
            .Include(e => e.User)
            .Include(e => e.Property)
            .ThenInclude(p => p.Address)
            .AsNoTracking();
    }
}
