using FernandaRentals.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FernandaRentals.Database.Configuration
{
    public class EventConfiguration : BaseConfiguration<EventEntity>
    {
        public void Configure(EntityTypeBuilder<EventEntity> builder)
        {
            base.Configure(builder);

            builder.Property(e => e.ClientId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(e => e.StartDate)
                .HasColumnName("start_date")
                .IsRequired();

            builder.Property(e => e.EndDate)
                .HasColumnName("end_date")
                .IsRequired();

            builder.Property(e => e.Location)
                .HasColumnName("location")
                .IsRequired();

            builder.Property(e => e.EventCost)
                .HasColumnName("subtotal")

                .HasPrecision(18,2)
                .HasColumnType("decimal(18,2)");

            builder.Property(e => e.Discount)
                .HasColumnName("discount")
                .HasPrecision(18,2)
                .HasColumnType("decimal(18,2)");

            builder.Property(e => e.Total)
                .HasColumnName("total")
                .HasPrecision(18,2)
                .HasColumnType("decimal(18,2)");

            // Relaciones
            builder.HasOne(e => e.Client)
                .WithMany()
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.EventDetails)
                .WithOne(d => d.Event)
                .HasForeignKey(d => d.EventId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
