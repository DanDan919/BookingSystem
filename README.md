Booking Service – Web API
 Project Overview

This project is a Web API for managing room bookings, built as a pet project to practice clean architecture, ASP.NET Web API, Entity Framework Core, and basic system design.

The system allows:

administrators to manage rooms

users to create and view bookings

prevention of double-booking for the same room and date range

The project follows a layered (clean) architecture approach.

 Technical Stack

.NET (ASP.NET Web API)

Entity Framework Core

PostgreSQL

Swagger / OpenAPI

Docker (configuration prepared)

C#

 Architecture

The solution is structured into separate layers:

Booking.Api
Web API layer (controllers, Swagger, HTTP pipeline)

Booking.Application
Application logic, DTOs, interfaces

Booking.Domain
Domain entities and domain rules

Booking.Infrastructure
EF Core, DbContext, repositories/services, database configuration

This separation keeps business logic isolated from infrastructure and delivery mechanisms.

 Functional Requirements (Technical Task)
Administrator functionality

Create a room

Delete a room

Get room information

Each room contains:

Class (e.g. Standard, Deluxe)

Price per day

Description

User functionality

Book a room for a specific date range

Get booking information

Cannot book a room if it is already booked for the selected dates

Additional requirements

Data persistence via Entity Framework Core

Database: PostgreSQL

Web API project

Layered architecture

Docker support

What Is Implemented

RESTful Web API

Room management (create, delete, get)

Booking creation with date range validation

Booking conflict detection (no overlapping bookings)

Entity Framework Core integration

PostgreSQL-compatible data model

Swagger UI for API exploration

Clean separation of layers (Domain / Application / Infrastructure / API)

Dockerfile and docker-compose configuration prepared

 Docker Status

Docker configuration files (Dockerfile, docker-compose.yml) are present.

However:

The application was NOT run locally inside Docker,
because hardware virtualization is disabled on the current machine.

The Docker setup is prepared for deployment and should work in an environment with virtualization support enabled.

 Running the Project
Local run (without Docker)

Configure the PostgreSQL connection string in appsettings.json

Apply migrations (if needed)

Run Booking.Api

Open Swagger UI:

https://localhost:{port}/swagger
