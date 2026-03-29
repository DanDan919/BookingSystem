namespace Booking.Application.DTO;

public class RoomFilterDto
{
    public string? Class { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}