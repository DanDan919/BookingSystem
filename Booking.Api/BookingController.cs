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
}