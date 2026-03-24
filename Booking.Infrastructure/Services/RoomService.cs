using Booking.Application.DTO;
using Booking.Application.Interfaces;
using Booking.Domain.Entities;
using Booking.Application.Exceptions;
using Booking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Booking.Infrastructure.Services;

public class RoomService : IRoomService
{
    private readonly BookingDbContext _dbContext;
    private readonly ILogger<RoomService> _logger;

    public RoomService(
        BookingDbContext dbContext,
        ILogger<RoomService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;

        _logger.LogInformation("RoomService создан");
    }

    public async Task<RoomDto> CreateRoomAsync(CreateRoomDto dto)
    {
        _logger.LogInformation(
            "CreateRoomAsync | Class={Class}, PricePerDay={Price}",
            dto.Class,
            dto.PricePerDay);

        if (string.IsNullOrWhiteSpace(dto.Class))
            throw new ValidationException("Класс комнаты обязателен");

        if (string.IsNullOrWhiteSpace(dto.Description))
            throw new ValidationException("Описание комнаты обязательно");

        if (dto.PricePerDay <= 0)
            throw new ValidationException("Цена комнаты должна быть больше нуля");

        var room = new Room(
            dto.Class,
            dto.PricePerDay,
            dto.Description
        );

        _dbContext.Rooms.Add(room);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Комната создана | RoomId={RoomId}", room.Id);

        return MapToDto(room);
    }

    public async Task DeleteRoomAsync(int roomId)
    {
        _logger.LogInformation("DeleteRoomAsync | RoomId={RoomId}", roomId);

        var room = await _dbContext.Rooms
       .FirstOrDefaultAsync(r => r.Id == roomId && !r.IsDeleted);

        if (room == null)
        {
            _logger.LogWarning("Комната не найдена | RoomId={RoomId}", roomId);
            throw new InvalidOperationException($"Комната {roomId} не найдена");
        }

        room.Delete();
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Комната удалена | RoomId={RoomId}", roomId);
    }

    public async Task<List<RoomDto>> GetAllAsync()
    {
        _logger.LogInformation("GetAllAsync");

        var rooms = await _dbContext.Rooms
            .AsNoTracking()
            .Where(r => !r.IsDeleted)
            .ToListAsync();

        return rooms.Select(MapToDto).ToList();
    }

    public async Task<RoomDto> UpdateRoomAsync(int roomId, UpdateRoomDto dto)
    {
        _logger.LogInformation(
            "UpdateRoomAsync | RoomId={RoomId}, Class={Class}, PricePerDay={Price}",
            roomId,
            dto.Class,
            dto.PricePerDay);

        var room = await _dbContext.Rooms
            .FirstOrDefaultAsync(r => r.Id == roomId && !r.IsDeleted);

        if (room == null)
        {
            _logger.LogWarning("Комната не найдена | RoomId={RoomId}", roomId);
            throw new InvalidOperationException($"Комната {roomId} не найдена");
        }

        room.Update(dto.Class, dto.PricePerDay, dto.Description);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Комната обновлена | RoomId={RoomId}", roomId);

        return MapToDto(room);
    }

    public async Task<RoomDto?> GetByIdAsync(int roomId)
    {
        _logger.LogInformation("GetByIdAsync | RoomId={RoomId}", roomId);

        var room = await _dbContext.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == roomId && !r.IsDeleted);

        return room == null ? null : MapToDto(room);
    }

    private static RoomDto MapToDto(Room room)
    {
        return new RoomDto
        {
            Id = room.Id,
            Class = room.Class,
            Description = room.Description,
            PricePerDay = room.PricePerDay
        };
    }
}