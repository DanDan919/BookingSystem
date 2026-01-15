namespace Booking.Domain.Entities;

public class Room
{
    public int Id { get; private set; }

    public string Class { get; private set; } = null!;
    public decimal PricePerDay { get; private set; }
    public string Description { get; private set; } = null!;

    public bool IsDeleted { get; private set; }

    private Room() { } // EF

    public Room(string roomClass, decimal pricePerDay, string description)
    {
        Class = roomClass;
        PricePerDay = pricePerDay;
        Description = description;
        IsDeleted = false;
    }

    public void Delete()
    {
        IsDeleted = true;
    }
}