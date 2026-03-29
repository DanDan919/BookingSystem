using Booking.Application.DTO;

namespace Booking.Application.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(CreateBookingDto dto);
    Task<BookingDto?> GetByIdAsync(int bookingId);
    Task<PagedResultDto<BookingDto>> GetByUserAsync(int userId, PagingDto paging);
    Task CancelAsync(int bookingId);
}