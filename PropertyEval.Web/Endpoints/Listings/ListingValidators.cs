using FastEndpoints;
using FluentValidation;
using PropertyEval.Application.DTOs;

namespace PropertyEval.Web.Endpoints.Listings;

public class CreateListingValidator : Validator<CreateListingRequest>
{
    public CreateListingValidator()
    {
        RuleFor(x => x.PropertyId)
            .GreaterThan(0).WithMessage("Property id must be valid.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.AskingPrice)
            .GreaterThan(0).WithMessage("Asking price must be greater than zero.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Listing status is not supported.");
    }
}

public class GetListingValidator : Validator<GetListingRequest>
{
    public GetListingValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Listing id must be valid.");
    }
}

public class GetListingsValidator : Validator<GetListingsRequest>
{
    public GetListingsValidator()
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
            .WithMessage("Listing status is not supported.");

        RuleFor(x => x.MinAskingPrice)
            .GreaterThan(0)
            .When(x => x.MinAskingPrice is not null)
            .WithMessage("Minimum asking price must be greater than zero.");

        RuleFor(x => x.MaxAskingPrice)
            .GreaterThan(0)
            .When(x => x.MaxAskingPrice is not null)
            .WithMessage("Maximum asking price must be greater than zero.");

        RuleFor(x => x.MaxAskingPrice)
            .GreaterThanOrEqualTo(x => x.MinAskingPrice!.Value)
            .When(x => x.MinAskingPrice is not null && x.MaxAskingPrice is not null)
            .WithMessage("Maximum asking price must be greater than or equal to minimum asking price.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than zero.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}
