using Booking.Application.DTO;
using Booking.Application.Interfaces;
using Booking.Infrastructure.Persistence;
using Booking.Domain.Entities;
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
        var roomExists = await _db.Rooms
            .AnyAsync(r => r.Id == dto.RoomId && !r.IsDeleted);

        if (!roomExists)
            throw new InvalidOperationException("Комната не существует");

        var conflict = await _db.Bookings
            .AnyAsync(b => b.RoomId == dto.RoomId &&
                           b.Intersects(dto.DateFrom, dto.DateTo));

        if (conflict)
            throw new InvalidOperationException("Комната уже забронирована");

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
        var booking = await _db.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        return booking == null ? null : MapToDto(booking);
    }

    public async Task<List<BookingDto>> GetByUserAsync(int userId)
    {
        var bookings = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .ToListAsync();

        return bookings.Select(MapToDto).ToList();
    }

    private static BookingDto MapToDto(BookingEntity booking)
    {
        return new BookingDto
        {
            Id = booking.Id,
            RoomId = booking.RoomId,
            UserId = booking.UserId,
            DateFrom = booking.DateFrom,
            DateTo = booking.DateTo
        };
    }
}