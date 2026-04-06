using Booking.Domain.Entities;

namespace Booking.Application.Interfaces;

public interface IRoomRepository
{
    Task AddAsync(Room room, CancellationToken cancellationToken = default);
    Task<Room?> GetByIdAsync(int roomId, CancellationToken cancellationToken = default);
    Task<Room?> GetActiveByIdAsync(int roomId, CancellationToken cancellationToken = default);
    Task<bool> ExistsActiveAsync(int roomId, CancellationToken cancellationToken = default);
    Task<List<Room>> GetDeletedAsync(CancellationToken cancellationToken = default);
    Task<List<Room>> GetByStatusAsync(RoomStatus status, CancellationToken cancellationToken = default);
    IQueryable<Room> Query();
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}