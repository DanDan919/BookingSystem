using Booking.Application.DTO;

namespace Booking.Application.Interfaces;

public interface IRoomService
{
    Task<RoomDto> CreateRoomAsync(CreateRoomDto dto);
    Task DeleteRoomAsync(int roomId);
    Task<List<RoomDto>> GetAllAsync();
    Task<RoomDto?> GetByIdAsync(int roomId);
    Task<RoomDto> UpdateRoomAsync(int roomId, UpdateRoomDto dto);
    Task<PagedResultDto<RoomDto>> GetFilteredAsync(RoomFilterDto filter);
    Task<RoomDto> UpdateStatusAsync(int roomId, UpdateRoomStatusDto dto);
    Task<List<RoomDto>> GetByStatusAsync(string status);
    Task<List<RoomDto>> GetDeletedAsync();
    Task<RoomDto> RestoreAsync(int roomId);
}