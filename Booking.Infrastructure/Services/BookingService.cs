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

        var roomExists = await _db.Rooms
            .AnyAsync(r => r.Id == dto.RoomId && !r.IsDeleted);

        if (!roomExists)
            throw new NotFoundException("Комната не существует");

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

        return MapToDto(booking);
    }

    public async Task<BookingDto?> GetByIdAsync(int bookingId)
    {
        if (bookingId <= 0)
            throw new ValidationException("BookingId должен быть больше нуля");

        var booking = await _db.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        return booking == null ? null : MapToDto(booking);
    }

    public async Task<List<BookingDto>> GetByUserAsync(int userId)
    {
        if (userId <= 0)
            throw new ValidationException("UserId должен быть больше нуля");

        var bookings = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .OrderBy(b => b.DateFrom)
            .ToListAsync();

        return bookings.Select(MapToDto).ToList();
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

    private static BookingDto MapToDto(BookingEntity booking)
    {
        return new BookingDto
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            UserId = booking.UserId,
            DateFrom = booking.DateFrom,
            DateTo = booking.DateTo,
            IsCancelled = booking.IsCancelled
        };
    }
}