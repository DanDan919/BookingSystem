using Booking.Application.DTO;

namespace Booking.Application.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(CreateBookingDto dto);
    Task<BookingDto?> GetByIdAsync(int bookingId);
    Task<PagedResultDto<BookingDto>> GetByUserAsync(int userId, PagingDto paging);
    Task<PagedResultDto<BookingDto>> GetActiveByUserAsync(int userId, PagingDto paging);
    Task<PagedResultDto<BookingDto>> GetCancelledByUserAsync(int userId, PagingDto paging);
    Task<PagedResultDto<BookingDto>> GetByRoomAsync(int roomId, PagingDto paging);
    Task CancelAsync(int bookingId);
}