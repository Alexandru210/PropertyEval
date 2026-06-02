using PropertyEval.Domain.Enums;

namespace PropertyEval.Application.DTOs;

public class CreateListingRequest
{
    public int PropertyId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal AskingPrice { get; set; }
    public ListingStatus Status { get; set; } = ListingStatus.Active;
}

public class GetListingRequest
{
    public int Id { get; set; }
}

public class GetListingsRequest
{
    public int? PropertyId { get; set; }
    public int? UserId { get; set; }
    public ListingStatus? Status { get; set; }
    public decimal? MinAskingPrice { get; set; }
    public decimal? MaxAskingPrice { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public record ListingResponse(
    int Id,
    int PropertyId,
    int UserId,
    string UserFullName,
    string Title,
    decimal AskingPrice,
    ListingStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    PropertyResponse Property
);
