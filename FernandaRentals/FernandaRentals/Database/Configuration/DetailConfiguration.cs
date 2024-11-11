using FernandaRentals.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FernandaRentals.Database.Configuration
{
    public class DetailConfiguration : BaseConfiguration<DetailEntity>
    {
        public void Configure(EntityTypeBuilder<DetailEntity> builder)
        {
            // Configuracion de Event
            builder.Property(e => e.EventId)
                .HasColumnName("event_id")  
                .IsRequired(); 
            // Relacion
            builder.HasOne(e => e.Event) 
                .WithOne() 
                .HasForeignKey<EventEntity>(e => e.Id)  
                .OnDelete(DeleteBehavior.Restrict);  

            // Productos
            builder.Property(e => e.ProductId)
                .HasColumnName("product_id")  
                .IsRequired();  

            builder.HasOne(e => e.Product)  
                .WithOne()  
                .HasForeignKey<ProductEntity>(e => e.Id)  
                .OnDelete(DeleteBehavior.Restrict); 

            // Quantity
            builder.Property(e => e.Quantity)
                .HasColumnName("quantity") 
                .IsRequired(); 

            // Precio Del Producto por Unidad
            builder.Property(e => e.UnitPrice)
                .HasColumnName("unit_price")  
                .IsRequired()
                .HasPrecision(18,2)
                .HasColumnType("decimal(18,2)");

        }
    }
}
