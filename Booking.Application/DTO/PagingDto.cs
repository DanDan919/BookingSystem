namespace Booking.Application.DTO;

public class PagingDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}