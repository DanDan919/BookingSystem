# BookingSystem

A pet project built to practice ASP.NET Core Web API, Entity Framework Core, PostgreSQL, layered architecture, and backend design fundamentals.

The project models a simple booking system where:

- administrators manage rooms
- users create and view bookings
- the system prevents overlapping bookings for the same room and date range

## Tech Stack

- C#
- .NET / ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Swagger / OpenAPI
- Docker / docker-compose

## Solution Structure

The solution is split into separate layers:

- **Booking.Api** — HTTP layer, controllers, middleware, Swagger, request pipeline
- **Booking.Application** — DTOs, interfaces, contracts, application-level abstractions
- **Booking.Domain** — entities and core business rules
- **Booking.Infrastructure** — EF Core, DbContext, services, persistence, dependency injection

This structure helps keep delivery concerns, business rules, and persistence concerns separated.

## Current Features

### Room Management

- create a room
- get all rooms
- get room by id
- update a room
- soft delete a room

Each room contains:

- `Class`
- `Description`
- `PricePerDay`

### Booking Management

- create a booking
- get booking by id
- validate date range
- prevent double-booking for the same room and overlapping dates
- prevent booking a deleted room

## Business Rules

- a booking must have a valid date range
- a room cannot be booked if another booking for the same room intersects the requested dates
- deleted rooms are excluded from active room queries
- deleted rooms cannot be used for new bookings

## API Endpoints

### Rooms

- `GET /api/rooms`
- `GET /api/rooms/{id}`
- `POST /api/rooms`
- `PUT /api/rooms/{id}`
- `DELETE /api/rooms/{id}`

### Bookings

- `POST /api/bookings`
- `GET /api/bookings/{id}`

## Example Requests

### Create Room

```json
{
  "class": "Standard",
  "description": "Room with one double bed",
  "pricePerDay": 15000
}
