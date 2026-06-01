using PropertyEval.Domain.Enums;

namespace PropertyEval.Domain.Entities;

public class Evaluation
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public decimal EvaluatedValue { get; set; }
    public EvaluationStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime EvaluationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
