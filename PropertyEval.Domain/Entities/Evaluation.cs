using PropertyEval.Domain.Enums;

namespace PropertyEval.Domain.Entities;

public class Evaluation
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public int RequestedByUserId { get; set; }
    public User RequestedByUser { get; set; } = null!;
    public int? EvaluatorUserId { get; set; }
    public User? EvaluatorUser { get; set; }
    public decimal EvaluatedValue { get; set; }
    public EvaluationStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime EvaluationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
