namespace Booking.Application.DTO;

public class CheckAvailabilityDto
{
    public int RoomId { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}