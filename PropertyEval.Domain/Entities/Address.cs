namespace PropertyEval.Domain.Entities;

public class Address
{
    public int Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
