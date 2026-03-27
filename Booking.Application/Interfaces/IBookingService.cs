using Booking.Application.DTO;

namespace Booking.Application.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(CreateBookingDto dto);
    Task<BookingDto?> GetByIdAsync(int bookingId);
    Task<List<BookingDto>> GetByUserAsync(int userId);
    Task CancelAsync(int bookingId);
}