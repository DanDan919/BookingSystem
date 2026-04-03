namespace Booking.Application.DTO;

public class BookingDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int UserId { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
    public bool IsCancelled { get; set; }

    public string? RoomClass { get; set; }
    public string? RoomStatus { get; set; }
    public decimal? RoomPricePerDay { get; set; }
}