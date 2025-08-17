using PropertyEval.Domain.Enums;

namespace PropertyEval.Domain.Entities
{
    public class Property
    {
        public int Id { get; set; }
        public Address Address { get; set; } = null!;
        public PropertyType PropertyType { get; set; }
        public int Area { get; set; }
        public int Bedrooms { get; set; }
        public int Bathrooms { get; set; }
        public int YearBuilt { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
