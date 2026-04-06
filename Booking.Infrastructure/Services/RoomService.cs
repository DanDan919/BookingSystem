using Booking.Application.Interfaces;
using Booking.Infrastructure.Persistence.Repositories;
using Booking.Application.DTO;
using Booking.Application.Exceptions;
using Booking.Application.Interfaces;
using Booking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Booking.Infrastructure.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<RoomService> _logger;

    public RoomService(
        IRoomRepository roomRepository,
        IBookingRepository bookingRepository,
        ILogger<RoomService> logger)
    {
        _roomRepository = roomRepository;
        _bookingRepository = bookingRepository;
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

        var room = new Room(dto.Class, dto.PricePerDay, dto.Description);

        await _roomRepository.AddAsync(room);
        await _roomRepository.SaveChangesAsync();

        return MapToDto(room);
    }

    public async Task DeleteRoomAsync(int roomId)
    {
        var room = await _roomRepository.GetActiveByIdAsync(roomId);

        if (room == null)
            throw new NotFoundException($"Комната {roomId} не найдена");

        room.Delete();
        await _roomRepository.SaveChangesAsync();
    }

    public async Task<RoomDto> RestoreAsync(int roomId)
    {
        var room = await _roomRepository.GetByIdAsync(roomId);

        if (room == null || !room.IsDeleted)
            throw new NotFoundException($"Удалённая комната {roomId} не найдена");

        room.Restore();
        await _roomRepository.SaveChangesAsync();

        return MapToDto(room);
    }

    public async Task<List<RoomDto>> GetAllAsync()
    {
        var rooms = await _roomRepository.Query()
            .AsNoTracking()
            .Where(r => !r.IsDeleted)
            .ToListAsync();

        return rooms.Select(MapToDto).ToList();
    }

    public async Task<PagedResultDto<RoomDto>> GetFilteredAsync(RoomFilterDto filter)
    {
        if (filter.Page <= 0)
            throw new ValidationException("Page должен быть больше нуля");

        if (filter.PageSize <= 0)
            throw new ValidationException("PageSize должен быть больше нуля");

        var query = _roomRepository.Query()
            .AsNoTracking()
            .Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.Class))
            query = query.Where(r => r.Class == filter.Class);

        if (filter.MinPrice.HasValue)
            query = query.Where(r => r.PricePerDay >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(r => r.PricePerDay <= filter.MaxPrice.Value);

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
        if (string.IsNullOrWhiteSpace(status))
            throw new ValidationException("Статус комнаты обязателен");

        if (!Enum.TryParse<RoomStatus>(status, true, out var parsedStatus))
            throw new ValidationException("Некорректный статус комнаты");

        var rooms = await _roomRepository.GetByStatusAsync(parsedStatus);
        return rooms.Select(MapToDto).ToList();
    }

    public async Task<List<RoomDto>> GetDeletedAsync()
    {
        var rooms = await _roomRepository.GetDeletedAsync();
        return rooms.Select(MapToDto).ToList();
    }

    public async Task<RoomDto> UpdateRoomAsync(int roomId, UpdateRoomDto dto)
    {
        ValidateRoom(dto.Class, dto.Description, dto.PricePerDay);

        var room = await _roomRepository.GetActiveByIdAsync(roomId);

        if (room == null)
            throw new NotFoundException($"Комната {roomId} не найдена");

        room.Update(dto.Class, dto.PricePerDay, dto.Description);
        await _roomRepository.SaveChangesAsync();

        return MapToDto(room);
    }

    public async Task<RoomDto?> GetByIdAsync(int roomId)
    {
        var room = await _roomRepository.Query()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == roomId && !r.IsDeleted);

        return room == null ? null : MapToDto(room);
    }

    public async Task<RoomDto> UpdateStatusAsync(int roomId, UpdateRoomStatusDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Status))
            throw new ValidationException("Статус комнаты обязателен");

        if (!Enum.TryParse<RoomStatus>(dto.Status, true, out var parsedStatus))
            throw new ValidationException("Некорректный статус комнаты");

        var room = await _roomRepository.GetActiveByIdAsync(roomId);

        if (room == null)
            throw new NotFoundException($"Комната {roomId} не найдена");

        room.ChangeStatus(parsedStatus);
        await _roomRepository.SaveChangesAsync();

        return MapToDto(room);
    }

    public async Task<RoomAvailabilityDto> CheckAvailabilityAsync(int roomId, RoomAvailabilityQueryDto query)
    {
        if (roomId <= 0)
            throw new ValidationException("RoomId должен быть больше нуля");

        if (query.DateFrom >= query.DateTo)
            throw new ValidationException("Дата начала должна быть раньше даты окончания");

        var room = await _roomRepository.GetActiveByIdAsync(roomId);

        if (room == null)
            throw new NotFoundException($"Комната {roomId} не найдена");

        if (room.Status != RoomStatus.Available)
        {
            return new RoomAvailabilityDto
            {
                RoomId = roomId,
                IsAvailable = false,
                Message = "Комната недоступна для бронирования",
                RoomStatus = room.Status.ToString()
            };
        }

        var hasConflict = await _bookingRepository.HasActiveConflictAsync(
            roomId,
            query.DateFrom,
            query.DateTo);

        return new RoomAvailabilityDto
        {
            RoomId = roomId,
            IsAvailable = !hasConflict,
            Message = hasConflict
                ? "Комната занята на выбранные даты"
                : "Комната доступна",
            RoomStatus = room.Status.ToString()
        };
    }

    public async Task<List<BookingCalendarItemDto>> GetCalendarAsync(int roomId)
    {
        if (roomId <= 0)
            throw new ValidationException("RoomId должен быть больше нуля");

        var room = await _roomRepository.GetByIdAsync(roomId);

        if (room == null)
            throw new NotFoundException($"Комната {roomId} не найдена");

        return await _bookingRepository.Query()
            .AsNoTracking()
            .Where(b => b.RoomId == roomId && !b.IsCancelled)
            .OrderBy(b => b.DateFrom)
            .Select(b => new BookingCalendarItemDto
            {
                DateFrom = b.DateFrom,
                DateTo = b.DateTo
            })
            .ToListAsync();
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