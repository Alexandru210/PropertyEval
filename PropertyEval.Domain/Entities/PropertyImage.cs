namespace PropertyEval.Domain.Entities;

public class PropertyImage
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public string ImageUrl { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; }
}
