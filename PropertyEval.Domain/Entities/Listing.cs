using PropertyEval.Domain.Enums;

namespace PropertyEval.Domain.Entities;

public class Listing
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public ListingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
