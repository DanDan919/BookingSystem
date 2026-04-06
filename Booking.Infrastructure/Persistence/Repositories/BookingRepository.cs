using Booking.Application.Interfaces;
using Booking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Persistence.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly BookingDbContext _dbContext;

    public BookingRepository(BookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(BookingEntity booking, CancellationToken cancellationToken = default)
    {
        await _dbContext.Bookings.AddAsync(booking, cancellationToken);
    }

    public async Task<BookingEntity?> GetByIdAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
    }

    public IQueryable<BookingEntity> Query()
    {
        return _dbContext.Bookings;
    }

    public async Task<bool> HasActiveConflictAsync(
        int roomId,
        DateTime dateFrom,
        DateTime dateTo,
        int? excludeBookingId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Bookings
            .Where(b => !b.IsCancelled && b.RoomId == roomId);

        if (excludeBookingId.HasValue)
        {
            query = query.Where(b => b.Id != excludeBookingId.Value);
        }

        return await query.AnyAsync(
            b => b.Intersects(dateFrom, dateTo),
            cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}