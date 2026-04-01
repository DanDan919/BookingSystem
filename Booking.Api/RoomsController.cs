using Booking.Application.DTO;
using Booking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Api.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] RoomFilterDto filter)
    {
        var rooms = await _roomService.GetFilteredAsync(filter);
        return Ok(rooms);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var room = await _roomService.GetByIdAsync(id);

        if (room == null)
            return NotFound();

        return Ok(room);
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        var rooms = await _roomService.GetByStatusAsync(status);
        return Ok(rooms);
    }

    [HttpGet("deleted")]
    public async Task<IActionResult> GetDeleted()
    {
        var rooms = await _roomService.GetDeletedAsync();
        return Ok(rooms);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoomDto request)
    {
        var createdRoom = await _roomService.CreateRoomAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdRoom.Id },
            createdRoom);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomDto request)
    {
        var updatedRoom = await _roomService.UpdateRoomAsync(id, request);
        return Ok(updatedRoom);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateRoomStatusDto request)
    {
        var updatedRoom = await _roomService.UpdateStatusAsync(id, request);
        return Ok(updatedRoom);
    }

    [HttpPatch("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        var restoredRoom = await _roomService.RestoreAsync(id);
        return Ok(restoredRoom);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _roomService.DeleteRoomAsync(id);
        return NoContent();
    }
}