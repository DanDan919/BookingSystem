using System.Text.Json;
using Booking.Application.Exceptions;

namespace Booking.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error");

            await WriteResponseAsync(
                context,
                StatusCodes.Status400BadRequest,
                ex.Message);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found");

            await WriteResponseAsync(
                context,
                StatusCodes.Status404NotFound,
                ex.Message);
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(ex, "Conflict error");

            await WriteResponseAsync(
                context,
                StatusCodes.Status409Conflict,
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            await WriteResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Internal server error");
        }
    }

    private static async Task WriteResponseAsync(
        HttpContext context,
        int statusCode,
        string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            message
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}