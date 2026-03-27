using Booking.Domain.Exceptions;

namespace Booking.Domain.Entities;

public class BookingEntity
{
    public int Id { get; private set; }

    public int RoomId { get; private set; }
    public int UserId { get; private set; }

    public DateTime DateFrom { get; private set; }
    public DateTime DateTo { get; private set; }

    public bool IsCancelled { get; private set; }

    private BookingEntity() { } // EF

    public BookingEntity(int roomId, int userId, DateTime dateFrom, DateTime dateTo)
    {
        if (dateFrom >= dateTo)
            throw new DomainException("Дата начала должна быть меньше даты окончания");

        RoomId = roomId;
        UserId = userId;
        DateFrom = dateFrom;
        DateTo = dateTo;
        IsCancelled = false;
    }

    public bool Intersects(DateTime from, DateTime to)
        => DateFrom < to && DateTo > from;

    public void Cancel()
    {
        if (IsCancelled)
            throw new DomainException("Бронь уже отменена");

        IsCancelled = true;
    }
}