using Booking.Application.DTO;
using Booking.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Booking.Api.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _service;

    public BookingController(IBookingService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
    {
        var booking = await _service.CreateAsync(dto);

        return CreatedAtAction(
            nameof(Get),
            new { id = booking.Id },
            booking);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var booking = await _service.GetByIdAsync(id);

        if (booking == null)
            return NotFound();

        return Ok(booking);
    }

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId, [FromQuery] PagingDto paging)
    {
        var bookings = await _service.GetByUserAsync(userId, paging);
        return Ok(bookings);
    }

    [HttpGet("user/{userId:int}/active")]
    public async Task<IActionResult> GetActiveByUser(int userId, [FromQuery] PagingDto paging)
    {
        var bookings = await _service.GetActiveByUserAsync(userId, paging);
        return Ok(bookings);
    }

    [HttpGet("user/{userId:int}/cancelled")]
    public async Task<IActionResult> GetCancelledByUser(int userId, [FromQuery] PagingDto paging)
    {
        var bookings = await _service.GetCancelledByUserAsync(userId, paging);
        return Ok(bookings);
    }

    [HttpGet("room/{roomId:int}")]
    public async Task<IActionResult> GetByRoom(int roomId, [FromQuery] PagingDto paging)
    {
        var bookings = await _service.GetByRoomAsync(roomId, paging);
        return Ok(bookings);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive([FromQuery] PagingDto paging)
    {
        var bookings = await _service.GetActiveAsync(paging);
        return Ok(bookings);
    }

    [HttpGet("cancelled")]
    public async Task<IActionResult> GetCancelled([FromQuery] PagingDto paging)
    {
        var bookings = await _service.GetCancelledAsync(paging);
        return Ok(bookings);
    }

    [HttpGet("by-date-range")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateRangeQueryDto query)
    {
        var bookings = await _service.GetByDateRangeAsync(query);
        return Ok(bookings);
    }

    [HttpPut("{id:int}/reschedule")]
    public async Task<IActionResult> Reschedule(int id, [FromBody] RescheduleBookingDto dto)
    {
        var booking = await _service.RescheduleAsync(id, dto);
        return Ok(booking);
    }

    [HttpGet("room/{roomId:int}/calendar")]
    public async Task<IActionResult> GetRoomCalendar(int roomId)
    {
        var result = await _service.GetRoomCalendarAsync(roomId);
        return Ok(result);
    }

    [HttpPost("check-availability")]
    public async Task<IActionResult> CheckAvailability([FromBody] CheckAvailabilityDto dto)
    {
        var result = await _service.CheckAvailabilityAsync(dto);
        return Ok(result);
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _service.CancelAsync(id);
        return NoContent();
    }
}