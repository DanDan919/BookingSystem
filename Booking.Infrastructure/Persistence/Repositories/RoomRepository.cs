using Booking.Application.Interfaces;
using Booking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Persistence.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly BookingDbContext _dbContext;

    public RoomRepository(BookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Room room, CancellationToken cancellationToken = default)
    {
        await _dbContext.Rooms.AddAsync(room, cancellationToken);
    }

    public async Task<Room?> GetByIdAsync(int roomId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Rooms
            .FirstOrDefaultAsync(r => r.Id == roomId, cancellationToken);
    }

    public async Task<Room?> GetActiveByIdAsync(int roomId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Rooms
            .FirstOrDefaultAsync(r => r.Id == roomId && !r.IsDeleted, cancellationToken);
    }

    public async Task<bool> ExistsActiveAsync(int roomId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Rooms
            .AnyAsync(r => r.Id == roomId && !r.IsDeleted, cancellationToken);
    }

    public async Task<List<Room>> GetDeletedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Rooms
            .AsNoTracking()
            .Where(r => r.IsDeleted)
            .OrderBy(r => r.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Room>> GetByStatusAsync(RoomStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Rooms
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.Status == status)
            .OrderBy(r => r.Id)
            .ToListAsync(cancellationToken);
    }

    public IQueryable<Room> Query()
    {
        return _dbContext.Rooms;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}