using FernandaRentals.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FernandaRentals.Database.Configuration
{
    public class ClientTypeConfiguration : IEntityTypeConfiguration<ClientTypeEntity>
    {
        public void Configure (EntityTypeBuilder<ClientTypeEntity> builder)
        {



            //Columna Descripcion
            builder.Property(e => e.Description)
                .HasColumnName("description")
                .IsRequired()
                .HasMaxLength(255);
            //Columna Descuento
            builder.Property(e => e.Discount)
                .HasColumnName("discount")
                .IsRequired()
                .HasPrecision(18, 2).HasColumnType("decimal(18,2)"); ; //precicion del campo

            // PROP Created By
            builder.Property(e => e.CreatedBy)
                .HasColumnName("created_by")
                .HasMaxLength(450);

            // PROP Cretaed Date
            builder.Property(e => e.CreatedDate)
                .HasColumnName("created_date")
                .HasColumnType("datetime");

            // Configuracion Update By
            builder.Property(e => e.UpdatedBy)
                .HasColumnName("updated_by")
                .HasMaxLength(450);

            // PROP undated Date
            builder.Property(e => e.UpdatedDate)
                .HasColumnName("updated_date")
                .HasColumnType("datetime");

            // Relacion de Usuarios de cRETAED
            builder.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .HasPrincipalKey(u => u.Id)
                .IsRequired();

            // Relacion de Uusraios Updated
            builder.HasOne(e => e.UpdatedByUser)
                .WithMany()
                .HasForeignKey(e => e.UpdatedBy)
                .HasPrincipalKey(u => u.Id)
                .IsRequired();
        }
    }
}
