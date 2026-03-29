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
}