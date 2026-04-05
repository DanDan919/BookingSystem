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

    Task<PagedResultDto<BookingDto>> GetActiveAsync(PagingDto paging);
    Task<PagedResultDto<BookingDto>> GetCancelledAsync(PagingDto paging);
    Task<PagedResultDto<BookingDto>> GetByDateRangeAsync(DateRangeQueryDto query);
    Task<BookingDto> RescheduleAsync(int bookingId, RescheduleBookingDto dto);

    Task<AvailabilityResultDto> CheckAvailabilityAsync(CheckAvailabilityDto dto);
    Task<List<BookingCalendarItemDto>> GetRoomCalendarAsync(int roomId);
    Task CancelAsync(int bookingId);
}