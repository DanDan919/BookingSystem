namespace Booking.Application.DTO;

public class RoomDto
{
    public int Id { get; set; }
    public string Class { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public string Status { get; set; } = string.Empty;
}