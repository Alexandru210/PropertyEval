using PropertyEval.Domain.Enums;

namespace PropertyEval.Domain.Entities;

public class Property
{
    public int Id { get; set; }
    public int AddressId { get; set; }
    public Address Address { get; set; } = null!;
    public PropertyType PropertyType { get; set; }
    public int Area { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public int YearBuilt { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<Listing> Listings { get; set; } = [];
    public ICollection<Evaluation> Evaluations { get; set; } = [];
    public ICollection<PropertyImage> Images { get; set; } = [];
}
