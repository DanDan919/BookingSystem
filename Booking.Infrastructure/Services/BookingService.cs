using Booking.Application.DTO;
using Booking.Application.Exceptions;
using Booking.Application.Interfaces;
using Booking.Domain.Entities;
using Booking.Domain.Exceptions;
using Booking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly BookingDbContext _db;

    public BookingService(BookingDbContext db)
    {
        _db = db;
    }

    public async Task<BookingDto> CreateAsync(CreateBookingDto dto)
    {
        if (dto.RoomId <= 0)
            throw new ValidationException("RoomId должен быть больше нуля");

        if (dto.UserId <= 0)
            throw new ValidationException("UserId должен быть больше нуля");

        if (dto.DateFrom >= dto.DateTo)
            throw new ValidationException("Дата начала должна быть раньше даты окончания");

        var room = await _db.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == dto.RoomId && !r.IsDeleted);

        if (room == null)
            throw new NotFoundException("Комната не существует");

        if (room.Status != RoomStatus.Available)
            throw new ConflictException("Комната недоступна для бронирования");

        var conflict = await _db.Bookings
            .AnyAsync(b => !b.IsCancelled &&
                           b.RoomId == dto.RoomId &&
                           b.Intersects(dto.DateFrom, dto.DateTo));

        if (conflict)
            throw new ConflictException("Комната уже забронирована");

        var booking = new BookingEntity(
            dto.RoomId,
            dto.UserId,
            dto.DateFrom,
            dto.DateTo
        );

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        return new BookingDto
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            UserId = booking.UserId,
            DateFrom = booking.DateFrom,
            DateTo = booking.DateTo,
            IsCancelled = booking.IsCancelled,
            RoomClass = room.Class,
            RoomStatus = room.Status.ToString(),
            RoomPricePerDay = room.PricePerDay
        };
    }

    public async Task<BookingDto?> GetByIdAsync(int bookingId)
    {
        if (bookingId <= 0)
            throw new ValidationException("BookingId должен быть больше нуля");

        return await BookingQuery()
            .FirstOrDefaultAsync(b => b.Id == bookingId);
    }

    public async Task<PagedResultDto<BookingDto>> GetByUserAsync(int userId, PagingDto paging)
    {
        return await GetByUserInternalAsync(userId, paging, null);
    }

    public async Task<PagedResultDto<BookingDto>> GetActiveByUserAsync(int userId, PagingDto paging)
    {
        return await GetByUserInternalAsync(userId, paging, false);
    }

    public async Task<PagedResultDto<BookingDto>> GetCancelledByUserAsync(int userId, PagingDto paging)
    {
        return await GetByUserInternalAsync(userId, paging, true);
    }

    public async Task<PagedResultDto<BookingDto>> GetByRoomAsync(int roomId, PagingDto paging)
    {
        if (roomId <= 0)
            throw new ValidationException("RoomId должен быть больше нуля");

        ValidatePaging(paging);

        var roomExists = await _db.Rooms
            .AsNoTracking()
            .AnyAsync(r => r.Id == roomId);

        if (!roomExists)
            throw new NotFoundException("Комната не найдена");

        var query = BookingQuery()
            .Where(b => b.RoomId == roomId)
            .OrderBy(b => b.DateFrom);

        var totalCount = await query.CountAsync();

        var bookings = await query
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .ToListAsync();

        return new PagedResultDto<BookingDto>
        {
            Items = bookings,
            TotalCount = totalCount,
            Page = paging.Page,
            PageSize = paging.PageSize
        };
    }

    public async Task<AvailabilityResultDto> CheckAvailabilityAsync(CheckAvailabilityDto dto)
    {
        if (dto.RoomId <= 0)
            throw new ValidationException("RoomId должен быть больше нуля");

        if (dto.DateFrom >= dto.DateTo)
            throw new ValidationException("Дата начала должна быть раньше даты окончания");

        var room = await _db.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == dto.RoomId && !r.IsDeleted);

        if (room == null)
            throw new NotFoundException("Комната не существует");

        if (room.Status != RoomStatus.Available)
        {
            return new AvailabilityResultDto
            {
                RoomId = dto.RoomId,
                IsAvailable = false,
                Message = "Комната недоступна для бронирования"
            };
        }

        var conflict = await _db.Bookings
            .AsNoTracking()
            .AnyAsync(b => !b.IsCancelled &&
                           b.RoomId == dto.RoomId &&
                           b.Intersects(dto.DateFrom, dto.DateTo));

        return new AvailabilityResultDto
        {
            RoomId = dto.RoomId,
            IsAvailable = !conflict,
            Message = conflict
                ? "Комната занята на выбранные даты"
                : "Комната доступна"
        };
    }

    public async Task<List<BookingCalendarItemDto>> GetRoomCalendarAsync(int roomId)
    {
        if (roomId <= 0)
            throw new ValidationException("RoomId должен быть больше нуля");

        var roomExists = await _db.Rooms
            .AsNoTracking()
            .AnyAsync(r => r.Id == roomId);

        if (!roomExists)
            throw new NotFoundException("Комната не найдена");

        return await _db.Bookings
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

    public async Task CancelAsync(int bookingId)
    {
        if (bookingId <= 0)
            throw new ValidationException("BookingId должен быть больше нуля");

        var booking = await _db.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            throw new NotFoundException("Бронь не найдена");

        if (booking.IsCancelled)
            throw new ConflictException("Бронь уже отменена");

        try
        {
            booking.Cancel();
        }
        catch (DomainException ex)
        {
            throw new ConflictException(ex.Message);
        }

        await _db.SaveChangesAsync();
    }

    private async Task<PagedResultDto<BookingDto>> GetByUserInternalAsync(int userId, PagingDto paging, bool? isCancelled)
    {
        if (userId <= 0)
            throw new ValidationException("UserId должен быть больше нуля");

        ValidatePaging(paging);

        var query = BookingQuery()
            .Where(b => b.UserId == userId);

        if (isCancelled.HasValue)
        {
            query = query.Where(b => b.IsCancelled == isCancelled.Value);
        }

        query = query.OrderBy(b => b.DateFrom);

        var totalCount = await query.CountAsync();

        var bookings = await query
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .ToListAsync();

        return new PagedResultDto<BookingDto>
        {
            Items = bookings,
            TotalCount = totalCount,
            Page = paging.Page,
            PageSize = paging.PageSize
        };
    }

    private IQueryable<BookingDto> BookingQuery()
    {
        return from b in _db.Bookings.AsNoTracking()
               join r in _db.Rooms.AsNoTracking()
                   on b.RoomId equals r.Id into roomGroup
               from r in roomGroup.DefaultIfEmpty()
               select new BookingDto
               {
                   Id = b.Id,
                   RoomId = b.RoomId,
                   UserId = b.UserId,
                   DateFrom = b.DateFrom,
                   DateTo = b.DateTo,
                   IsCancelled = b.IsCancelled,
                   RoomClass = r != null ? r.Class : null,
                   RoomStatus = r != null ? r.Status.ToString() : null,
                   RoomPricePerDay = r != null ? r.PricePerDay : null
               };
    }

    private static void ValidatePaging(PagingDto paging)
    {
        if (paging.Page <= 0)
            throw new ValidationException("Page должен быть больше нуля");

        if (paging.PageSize <= 0)
            throw new ValidationException("PageSize должен быть больше нуля");
    }
}