namespace Booking.Application.DTO;

public class RoomDto
{
    public int Id { get; set; }

    public string Class { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal PricePerDay { get; set; }
    
}