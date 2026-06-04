using PropertyEval.Domain.Enums;

namespace PropertyEval.Application.DTOs;

public class CreateEvaluationRequest
{
    public int PropertyId { get; set; }
    public string? Notes { get; set; }
}

public class AssignEvaluationRequest
{
    public int Id { get; set; }
    public int EvaluatorUserId { get; set; }
}

public class CompleteEvaluationRequest
{
    public int Id { get; set; }
    public decimal EvaluatedValue { get; set; }
    public string? Notes { get; set; }
    public DateTime? EvaluationDate { get; set; }
}

public class GetEvaluationRequest
{
    public int Id { get; set; }
}

public class GetEvaluationsRequest
{
    public int? PropertyId { get; set; }
    public int? RequestedByUserId { get; set; }
    public int? EvaluatorUserId { get; set; }
    public EvaluationStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public record EvaluationResponse(
    int Id,
    int PropertyId,
    int RequestedByUserId,
    string RequestedByUserFullName,
    int? EvaluatorUserId,
    string? EvaluatorUserFullName,
    decimal EvaluatedValue,
    EvaluationStatus Status,
    string? Notes,
    DateTime EvaluationDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    PropertyResponse Property
);
