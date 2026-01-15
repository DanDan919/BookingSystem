using Microsoft.EntityFrameworkCore;
using Booking.Domain.Entities;

namespace Booking.Infrastructure.Persistence;

public class BookingDbContext : DbContext
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<BookingEntity> Bookings => Set<BookingEntity>();

    public BookingDbContext(DbContextOptions<BookingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ---------- Room ----------
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.Class)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(r => r.Description)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(r => r.PricePerDay)
                  .HasPrecision(10, 2);

            entity.Property(r => r.IsDeleted)
                  .IsRequired();
        });

        // ---------- Booking ----------
        modelBuilder.Entity<BookingEntity>(entity =>
        {
            entity.HasKey(b => b.Id);

            entity.Property(b => b.DateFrom)
                  .IsRequired();

            entity.Property(b => b.DateTo)
                  .IsRequired();

            entity.Property(b => b.UserId)
                  .IsRequired()
                  .HasMaxLength(100);

            // FK → Room
            entity.HasOne<Room>()
                  .WithMany()
                  .HasForeignKey(b => b.RoomId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Индекс для ускорения поиска пересечений
            entity.HasIndex(b => new { b.RoomId, b.DateFrom, b.DateTo });

            // Защита на уровне БД
            entity.ToTable(t =>
                t.HasCheckConstraint(
                    "CK_Booking_DateRange",
                    "\"DateFrom\" < \"DateTo\""
                ));
        });
    }
}