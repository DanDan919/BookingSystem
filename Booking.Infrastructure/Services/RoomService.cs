using Booking.Application.DTO;
using Booking.Application.Exceptions;
using Booking.Application.Interfaces;
using Booking.Domain.Entities;
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

        ValidateRoom(dto.Class, dto.Description, dto.PricePerDay);

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
            throw new NotFoundException($"Комната {roomId} не найдена");
        }

        room.Delete();
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Комната удалена | RoomId={RoomId}", roomId);
    }

    public async Task<RoomDto> RestoreAsync(int roomId)
    {
        _logger.LogInformation("RestoreAsync | RoomId={RoomId}", roomId);

        var room = await _dbContext.Rooms
            .FirstOrDefaultAsync(r => r.Id == roomId && r.IsDeleted);

        if (room == null)
        {
            _logger.LogWarning("Удалённая комната не найдена | RoomId={RoomId}", roomId);
            throw new NotFoundException($"Удалённая комната {roomId} не найдена");
        }

        room.Restore();
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Комната восстановлена | RoomId={RoomId}", roomId);

        return MapToDto(room);
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

    public async Task<PagedResultDto<RoomDto>> GetFilteredAsync(RoomFilterDto filter)
    {
        _logger.LogInformation(
            "GetFilteredAsync | Class={Class}, MinPrice={MinPrice}, MaxPrice={MaxPrice}, SortBy={SortBy}, Page={Page}, PageSize={PageSize}",
            filter.Class,
            filter.MinPrice,
            filter.MaxPrice,
            filter.SortBy,
            filter.Page,
            filter.PageSize);

        if (filter.Page <= 0)
            throw new ValidationException("Page должен быть больше нуля");

        if (filter.PageSize <= 0)
            throw new ValidationException("PageSize должен быть больше нуля");

        var query = _dbContext.Rooms
            .AsNoTracking()
            .Where(r => !r.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Class))
        {
            query = query.Where(r => r.Class == filter.Class);
        }

        if (filter.MinPrice.HasValue)
        {
            query = query.Where(r => r.PricePerDay >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(r => r.PricePerDay <= filter.MaxPrice.Value);
        }

        query = filter.SortBy switch
        {
            "priceAsc" => query.OrderBy(r => r.PricePerDay),
            "priceDesc" => query.OrderByDescending(r => r.PricePerDay),
            _ => query.OrderBy(r => r.Id)
        };

        var totalCount = await query.CountAsync();

        var rooms = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResultDto<RoomDto>
        {
            Items = rooms.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<List<RoomDto>> GetByStatusAsync(string status)
    {
        _logger.LogInformation("GetByStatusAsync | Status={Status}", status);

        if (string.IsNullOrWhiteSpace(status))
            throw new ValidationException("Статус комнаты обязателен");

        if (!Enum.TryParse<RoomStatus>(status, true, out var parsedStatus))
            throw new ValidationException("Некорректный статус комнаты");

        var rooms = await _dbContext.Rooms
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.Status == parsedStatus)
            .OrderBy(r => r.Id)
            .ToListAsync();

        return rooms.Select(MapToDto).ToList();
    }

    public async Task<List<RoomDto>> GetDeletedAsync()
    {
        _logger.LogInformation("GetDeletedAsync");

        var rooms = await _dbContext.Rooms
            .AsNoTracking()
            .Where(r => r.IsDeleted)
            .OrderBy(r => r.Id)
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

        ValidateRoom(dto.Class, dto.Description, dto.PricePerDay);

        var room = await _dbContext.Rooms
            .FirstOrDefaultAsync(r => r.Id == roomId && !r.IsDeleted);

        if (room == null)
        {
            _logger.LogWarning("Комната не найдена | RoomId={RoomId}", roomId);
            throw new NotFoundException($"Комната {roomId} не найдена");
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

    public async Task<RoomDto> UpdateStatusAsync(int roomId, UpdateRoomStatusDto dto)
    {
        _logger.LogInformation(
            "UpdateStatusAsync | RoomId={RoomId}, Status={Status}",
            roomId,
            dto.Status);

        if (string.IsNullOrWhiteSpace(dto.Status))
            throw new ValidationException("Статус комнаты обязателен");

        if (!Enum.TryParse<RoomStatus>(dto.Status, true, out var parsedStatus))
            throw new ValidationException("Некорректный статус комнаты");

        var room = await _dbContext.Rooms
            .FirstOrDefaultAsync(r => r.Id == roomId && !r.IsDeleted);

        if (room == null)
        {
            _logger.LogWarning("Комната не найдена | RoomId={RoomId}", roomId);
            throw new NotFoundException($"Комната {roomId} не найдена");
        }

        room.ChangeStatus(parsedStatus);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Статус комнаты обновлён | RoomId={RoomId}, Status={Status}",
            roomId,
            parsedStatus);

        return MapToDto(room);
    }

    private static void ValidateRoom(string roomClass, string description, decimal pricePerDay)
    {
        if (string.IsNullOrWhiteSpace(roomClass))
            throw new ValidationException("Класс комнаты обязателен");

        if (string.IsNullOrWhiteSpace(description))
            throw new ValidationException("Описание комнаты обязательно");

        if (pricePerDay <= 0)
            throw new ValidationException("Цена комнаты должна быть больше нуля");
    }

    private static RoomDto MapToDto(Room room)
    {
        return new RoomDto
        {
            Id = room.Id,
            Class = room.Class,
            Description = room.Description,
            PricePerDay = room.PricePerDay,
            Status = room.Status.ToString()
        };
    }
}