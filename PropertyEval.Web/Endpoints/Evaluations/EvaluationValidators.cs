using FastEndpoints;
using FluentValidation;
using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Enums;

namespace PropertyEval.Web.Endpoints.Evaluations;

public class CreateEvaluationValidator : Validator<CreateEvaluationRequest>
{
    public CreateEvaluationValidator()
    {
        RuleFor(x => x.PropertyId)
            .GreaterThan(0).WithMessage("Property id must be valid.");

        RuleFor(x => x.EvaluatedValue)
            .GreaterThanOrEqualTo(0)
            .When(x => x.EvaluatedValue is not null)
            .WithMessage("Evaluated value cannot be negative.");

        RuleFor(x => x.EvaluatedValue)
            .NotNull()
            .GreaterThan(0)
            .When(x => x.Status == EvaluationStatus.Completed)
            .WithMessage("Completed evaluations must include a positive evaluated value.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Evaluation status is not supported.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters.");
    }
}

public class GetEvaluationValidator : Validator<GetEvaluationRequest>
{
    public GetEvaluationValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Evaluation id must be valid.");
    }
}

public class GetEvaluationsValidator : Validator<GetEvaluationsRequest>
{
    public GetEvaluationsValidator()
    {
        RuleFor(x => x.PropertyId)
            .GreaterThan(0)
            .When(x => x.PropertyId is not null)
            .WithMessage("Property id must be valid.");

        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .When(x => x.UserId is not null)
            .WithMessage("User id must be valid.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status is not null)
            .WithMessage("Evaluation status is not supported.");

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate!.Value)
            .When(x => x.FromDate is not null && x.ToDate is not null)
            .WithMessage("To date must be greater than or equal to from date.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than zero.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}
