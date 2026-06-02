using PropertyEval.Domain.Enums;

namespace PropertyEval.Application.DTOs;

public record AddressRequest(
    string Street,
    string City,
    string County
);

public record AddressResponse(
    int Id,
    string Street,
    string City,
    string County
);

public class CreatePropertyRequest
{
    public AddressRequest Address { get; set; } = new(string.Empty, string.Empty, string.Empty);
    public PropertyType PropertyType { get; set; }
    public int Area { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int YearBuilt { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class GetPropertyRequest
{
    public int Id { get; set; }
}

public class GetPropertiesRequest
{
    public PropertyType? PropertyType { get; set; }
    public string? City { get; set; }
    public string? County { get; set; }
    public int? MinArea { get; set; }
    public int? MaxArea { get; set; }
    public int? MinBedrooms { get; set; }
    public int? MinBathrooms { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public record PropertyResponse(
    int Id,
    AddressResponse Address,
    PropertyType PropertyType,
    int Area,
    int Bedrooms,
    int Bathrooms,
    int YearBuilt,
    string Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
