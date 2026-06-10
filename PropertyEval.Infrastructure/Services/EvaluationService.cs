using Microsoft.EntityFrameworkCore;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Common;
using PropertyEval.Domain.Constants;
using PropertyEval.Domain.Entities;
using PropertyEval.Domain.Enums;
using PropertyEval.Infrastructure.Data;

namespace PropertyEval.Infrastructure.Services;

public class EvaluationService
{
    private readonly AppDbContext _context;

    public EvaluationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EvaluationResponse> CreateEvaluationAsync(
        CreateEvaluationRequest request,
        int requestedByUserId,
        bool canUseAnyProperty,
        CancellationToken cancellationToken)
    {
        var property = await _context.Properties
            .Include(p => p.Address)
            .Include(p => p.Images)
            .SingleOrDefaultAsync(p => p.Id == request.PropertyId, cancellationToken);

        if (property is null)
        {
            throw new KeyNotFoundException("Property was not found.");
        }

        if (!canUseAnyProperty && property.OwnerUserId != requestedByUserId)
        {
            throw new ForbiddenAccessException("You can only request evaluations for properties you created.");
        }

        if (property.Images.Count == 0)
        {
            throw new InvalidOperationException("Evaluation requests must include at least one property photo.");
        }

        var requestedByUser = await _context.Users
            .SingleOrDefaultAsync(u => u.Id == requestedByUserId, cancellationToken);

        if (requestedByUser is null)
        {
            throw new KeyNotFoundException("User was not found.");
        }

        var now = DateTime.UtcNow;
        var evaluation = new Evaluation
        {
            PropertyId = property.Id,
            Property = property,
            RequestedByUserId = requestedByUser.Id,
            RequestedByUser = requestedByUser,
            EvaluatedValue = 0m,
            Status = EvaluationStatus.Pending,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            EvaluationDate = now,
            CreatedAt = now
        };

        _context.Evaluations.Add(evaluation);
        await _context.SaveChangesAsync(cancellationToken);

        return ResponseMapper.ToResponse(evaluation);
    }

    public async Task<EvaluationResponse> AssignEvaluationAsync(
        int evaluationId,
        int evaluatorUserId,
        CancellationToken cancellationToken)
    {
        var evaluation = await CreateEvaluationQuery(asNoTracking: false)
            .SingleOrDefaultAsync(e => e.Id == evaluationId, cancellationToken);

        if (evaluation is null)
        {
            throw new KeyNotFoundException("Evaluation was not found.");
        }

        if (evaluation.Status == EvaluationStatus.Completed)
        {
            throw new InvalidOperationException("Completed evaluations cannot be reassigned.");
        }

        var evaluator = await _context.Users
            .Include(u => u.Role)
            .SingleOrDefaultAsync(u => u.Id == evaluatorUserId, cancellationToken);

        if (evaluator is null)
        {
            throw new KeyNotFoundException("Evaluator user was not found.");
        }

        if (evaluator.Role.Name != SystemRoles.Evaluator)
        {
            throw new InvalidOperationException("Assigned user must have the Evaluator role.");
        }

        evaluation.EvaluatorUserId = evaluator.Id;
        evaluation.EvaluatorUser = evaluator;
        evaluation.Status = EvaluationStatus.InProgress;
        evaluation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return ResponseMapper.ToResponse(evaluation);
    }

    public async Task<EvaluationResponse> CompleteEvaluationAsync(
        CompleteEvaluationRequest request,
        int userId,
        bool canCompleteAnyEvaluation,
        CancellationToken cancellationToken)
    {
        if (request.EvaluatedValue <= 0m)
        {
            throw new InvalidOperationException("Completed evaluations must include a positive evaluated value.");
        }

        var query = CreateEvaluationQuery(asNoTracking: false);

        if (!canCompleteAnyEvaluation)
        {
            query = query.Where(e => e.EvaluatorUserId == userId);
        }

        var evaluation = await query.SingleOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (evaluation is null)
        {
            throw new KeyNotFoundException("Evaluation was not found.");
        }

        var now = DateTime.UtcNow;
        evaluation.EvaluatedValue = request.EvaluatedValue;
        evaluation.Status = EvaluationStatus.Completed;
        evaluation.EvaluationDate = request.EvaluationDate ?? now;
        evaluation.UpdatedAt = now;

        if (request.Notes is not null)
        {
            evaluation.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return ResponseMapper.ToResponse(evaluation);
    }

    public async Task<EvaluationResponse> GetEvaluationAsync(
        int id,
        int userId,
        bool canViewAllEvaluations,
        bool canViewAssignedEvaluations,
        CancellationToken cancellationToken)
    {
        var query = ApplyEvaluationAccess(
            CreateEvaluationQuery(),
            userId,
            canViewAllEvaluations,
            canViewAssignedEvaluations);

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
        bool canViewAssignedEvaluations,
        CancellationToken cancellationToken)
    {
        var query = ApplyEvaluationAccess(
            CreateEvaluationQuery(),
            userId,
            canViewAllEvaluations,
            canViewAssignedEvaluations);

        if (canViewAllEvaluations && request.RequestedByUserId is not null)
        {
            query = query.Where(e => e.RequestedByUserId == request.RequestedByUserId);
        }

        if (canViewAllEvaluations && request.EvaluatorUserId is not null)
        {
            query = query.Where(e => e.EvaluatorUserId == request.EvaluatorUserId);
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

    private static IQueryable<Evaluation> ApplyEvaluationAccess(
        IQueryable<Evaluation> query,
        int userId,
        bool canViewAllEvaluations,
        bool canViewAssignedEvaluations)
    {
        if (canViewAllEvaluations)
        {
            return query;
        }

        if (canViewAssignedEvaluations)
        {
            return query.Where(e => e.EvaluatorUserId == userId);
        }

        return query.Where(e => e.RequestedByUserId == userId);
    }

    private IQueryable<Evaluation> CreateEvaluationQuery(bool asNoTracking = true)
    {
        var query = _context.Evaluations
            .Include(e => e.RequestedByUser)
            .Include(e => e.EvaluatorUser)
            .Include(e => e.Property)
                .ThenInclude(p => p.Address)
            .Include(e => e.Property)
                .ThenInclude(p => p.Images);

        return asNoTracking ? query.AsNoTracking() : query;
    }
}
