namespace Booking.Application.DTO;

public class AvailabilityResultDto
{
    public int RoomId { get; set; }
    public bool IsAvailable { get; set; }
    public string Message { get; set; } = string.Empty;
}