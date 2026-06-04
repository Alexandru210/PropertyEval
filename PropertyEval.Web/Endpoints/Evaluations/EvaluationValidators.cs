using FastEndpoints;
using FluentValidation;
using PropertyEval.Application.DTOs;

namespace PropertyEval.Web.Endpoints.Evaluations;

public class CreateEvaluationValidator : Validator<CreateEvaluationRequest>
{
    public CreateEvaluationValidator()
    {
        RuleFor(x => x.PropertyId)
            .GreaterThan(0).WithMessage("Property id must be valid.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters.");
    }
}

public class AssignEvaluationValidator : Validator<AssignEvaluationRequest>
{
    public AssignEvaluationValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Evaluation id must be valid.");

        RuleFor(x => x.EvaluatorUserId)
            .GreaterThan(0).WithMessage("Evaluator user id must be valid.");
    }
}

public class CompleteEvaluationValidator : Validator<CompleteEvaluationRequest>
{
    public CompleteEvaluationValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Evaluation id must be valid.");

        RuleFor(x => x.EvaluatedValue)
            .GreaterThan(0).WithMessage("Completed evaluations must include a positive evaluated value.");

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

        RuleFor(x => x.RequestedByUserId)
            .GreaterThan(0)
            .When(x => x.RequestedByUserId is not null)
            .WithMessage("Requester user id must be valid.");

        RuleFor(x => x.EvaluatorUserId)
            .GreaterThan(0)
            .When(x => x.EvaluatorUserId is not null)
            .WithMessage("Evaluator user id must be valid.");

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
