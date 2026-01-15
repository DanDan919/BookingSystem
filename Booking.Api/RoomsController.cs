using Booking.Domain.Entities;
using Booking.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booking.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly BookingDbContext _db;

    public RoomsController(BookingDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rooms = await _db.Rooms.ToListAsync();
        return Ok(rooms);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Room room)
    {
        _db.Rooms.Add(room);
        await _db.SaveChangesAsync();

        return Ok(room);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var room = await _db.Rooms.FindAsync(id);
        if (room == null)
            return NotFound();

        _db.Rooms.Remove(room);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}