using PropertyEval.Application.DTOs;
using PropertyEval.Domain.Entities;

namespace PropertyEval.Infrastructure.Services;

internal static class ResponseMapper
{
    public static AddressResponse ToResponse(Address address)
    {
        return new AddressResponse(
            address.Id,
            address.Street,
            address.City,
            address.County
        );
    }

    public static PropertyResponse ToResponse(Property property)
    {
        return new PropertyResponse(
            property.Id,
            ToResponse(property.Address),
            property.PropertyType,
            property.Area,
            property.Bedrooms,
            property.Bathrooms,
            property.YearBuilt,
            property.Description,
            property.CreatedAt,
            property.UpdatedAt
        );
    }

    public static ListingResponse ToResponse(Listing listing)
    {
        return new ListingResponse(
            listing.Id,
            listing.PropertyId,
            listing.UserId,
            GetFullName(listing.User),
            listing.Title,
            listing.AskingPrice,
            listing.Status,
            listing.CreatedAt,
            listing.UpdatedAt,
            ToResponse(listing.Property)
        );
    }

    public static EvaluationResponse ToResponse(Evaluation evaluation)
    {
        return new EvaluationResponse(
            evaluation.Id,
            evaluation.PropertyId,
            evaluation.RequestedByUserId,
            GetFullName(evaluation.RequestedByUser),
            evaluation.EvaluatorUserId,
            GetOptionalFullName(evaluation.EvaluatorUser),
            evaluation.EvaluatedValue,
            evaluation.Status,
            evaluation.Notes,
            evaluation.EvaluationDate,
            evaluation.CreatedAt,
            evaluation.UpdatedAt,
            ToResponse(evaluation.Property)
        );
    }

    private static string GetFullName(User user)
    {
        return $"{user.FirstName} {user.LastName}".Trim();
    }

    private static string? GetOptionalFullName(User? user)
    {
        return user is null ? null : GetFullName(user);
    }
}
