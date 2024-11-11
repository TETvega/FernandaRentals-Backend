using FernandaRentals.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FernandaRentals.Database.Configuration
{
    public class ProductConfiguration : BaseConfiguration<ProductEntity>
    {
        public void Configure(EntityTypeBuilder<ProductEntity> builder)
        {
            base.Configure(builder);


            // Descripcion
            builder.Property(p => p.Description)
                .HasColumnName("description")
                .HasMaxLength(501)
                .IsRequired();
            // IMAGEN URL
            builder.Property(p => p.UrlImage)
                .HasColumnName("url_image")
                .IsRequired();
            // CATEGOPRIA
            builder.Property(p => p.CategoryId)
                .HasColumnName("category_id")
                .IsRequired();
            // STOCK
            builder.Property(p => p.Stock)
                .HasColumnName("stock")
                .IsRequired();
            // COSTO UNITARIO
            builder.Property(p => p.Cost)
                .HasColumnName("cost")
                .IsRequired()
                .HasPrecision(18,2);

            // RELACION CATEGORIA
            builder.HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); 

            
        }
    }
}
