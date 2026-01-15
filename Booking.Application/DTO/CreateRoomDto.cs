namespace Booking.Application.DTO;

public class CreateRoomDto
{
    public string Class { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal PricePerDay { get; set; }
}