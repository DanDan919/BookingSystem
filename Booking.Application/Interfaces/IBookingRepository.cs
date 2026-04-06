using Booking.Domain.Entities;

namespace Booking.Application.Interfaces;

public interface IBookingRepository
{
    Task AddAsync(BookingEntity booking, CancellationToken cancellationToken = default);
    Task<BookingEntity?> GetByIdAsync(int bookingId, CancellationToken cancellationToken = default);
    IQueryable<BookingEntity> Query();
    Task<bool> HasActiveConflictAsync(int roomId, DateTime dateFrom, DateTime dateTo, int? excludeBookingId = null, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}