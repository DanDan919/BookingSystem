namespace Booking.Application.DTO;

public class CreateBookingDto
{
    public int RoomId { get; set; }
    public int UserId { get; set; } 
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}