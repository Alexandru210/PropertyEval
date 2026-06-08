using FastEndpoints;
using FluentValidation;
using PropertyEval.Application.DTOs;

namespace PropertyEval.Web.Endpoints.Properties;

public class AddressRequestValidator : Validator<AddressRequest>
{
    public AddressRequestValidator()
    {
        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required.")
            .MaximumLength(200).WithMessage("Street cannot exceed 200 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.County)
            .NotEmpty().WithMessage("County is required.")
            .MaximumLength(100).WithMessage("County cannot exceed 100 characters.");
    }
}

public class CreatePropertyValidator : Validator<CreatePropertyRequest>
{
    public CreatePropertyValidator()
    {
        RuleFor(x => x.Address)
            .NotNull().WithMessage("Address is required.")
            .SetValidator(new AddressRequestValidator());

        RuleFor(x => x.PropertyType)
            .IsInEnum().WithMessage("Property type is not supported.");

        RuleFor(x => x.Area)
            .GreaterThan(0).WithMessage("Area must be greater than zero.");

        RuleFor(x => x.Bedrooms)
            .GreaterThanOrEqualTo(0).WithMessage("Bedrooms cannot be negative.");

        RuleFor(x => x.Bathrooms)
            .GreaterThanOrEqualTo(0).WithMessage("Bathrooms cannot be negative.");

        RuleFor(x => x.YearBuilt)
            .InclusiveBetween(1800, DateTime.UtcNow.Year + 1)
            .WithMessage("Year built must be realistic.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");
    }
}

public class GetPropertyValidator : Validator<GetPropertyRequest>
{
    public GetPropertyValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Property id must be valid.");
    }
}

public class UploadPropertyImagesValidator : Validator<UploadPropertyImagesRequest>
{
    public UploadPropertyImagesValidator()
    {
        RuleFor(x => x.PropertyId)
            .GreaterThan(0).WithMessage("Property id must be valid.");
    }
}

public class DeletePropertyImageValidator : Validator<DeletePropertyImageRequest>
{
    public DeletePropertyImageValidator()
    {
        RuleFor(x => x.PropertyId)
            .GreaterThan(0).WithMessage("Property id must be valid.");

        RuleFor(x => x.ImageId)
            .GreaterThan(0).WithMessage("Image id must be valid.");
    }
}

public class GetPropertyValuationValidator : Validator<GetPropertyValuationRequest>
{
    public GetPropertyValuationValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Property id must be valid.");
    }
}

public class GetPropertiesValidator : Validator<GetPropertiesRequest>
{
    public GetPropertiesValidator()
    {
        RuleFor(x => x.PropertyType)
            .IsInEnum()
            .When(x => x.PropertyType is not null)
            .WithMessage("Property type is not supported.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.County)
            .MaximumLength(100).WithMessage("County cannot exceed 100 characters.");

        RuleFor(x => x.MinArea)
            .GreaterThan(0)
            .When(x => x.MinArea is not null)
            .WithMessage("Minimum area must be greater than zero.");

        RuleFor(x => x.MaxArea)
            .GreaterThan(0)
            .When(x => x.MaxArea is not null)
            .WithMessage("Maximum area must be greater than zero.");

        RuleFor(x => x.MaxArea)
            .GreaterThanOrEqualTo(x => x.MinArea!.Value)
            .When(x => x.MinArea is not null && x.MaxArea is not null)
            .WithMessage("Maximum area must be greater than or equal to minimum area.");

        RuleFor(x => x.MinBedrooms)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinBedrooms is not null)
            .WithMessage("Minimum bedrooms cannot be negative.");

        RuleFor(x => x.MinBathrooms)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinBathrooms is not null)
            .WithMessage("Minimum bathrooms cannot be negative.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than zero.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}
