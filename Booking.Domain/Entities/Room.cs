namespace Booking.Domain.Entities;

public class Room
{
    public int Id { get; private set; }
    public string Class { get; private set; }
    public decimal PricePerDay { get; private set; }
    public string Description { get; private set; }
    public bool IsDeleted { get; private set; }
    public RoomStatus Status { get; private set; }

    private Room() { } // EF

    public Room(string roomClass, decimal pricePerDay, string description)
    {
        Class = roomClass;
        PricePerDay = pricePerDay;
        Description = description;
        IsDeleted = false;
        Status = RoomStatus.Available;
    }

    public void Delete()
    {
        IsDeleted = true;
    }

    public void Restore()
    {
        IsDeleted = false;
    }

    public void Update(string roomClass, decimal pricePerDay, string description)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Нельзя изменить удалённую комнату");

        Class = roomClass;
        PricePerDay = pricePerDay;
        Description = description;
    }

    public void ChangeStatus(RoomStatus status)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Нельзя изменить статус удалённой комнаты");

        Status = status;
    }
}