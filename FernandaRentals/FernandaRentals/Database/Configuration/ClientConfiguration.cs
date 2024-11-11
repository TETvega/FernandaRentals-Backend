using FernandaRentals.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FernandaRentals.Database.Configuration
{
    public class ClientConfiguration : BaseConfiguration<ClientEntity>
    {
        public void Configure(EntityTypeBuilder<ClientEntity> builder)
        {
            // Propiedad ID
            //builder.HasKey(e => e.Id);
           // builder.Property(e => e.Id)
             //   .HasColumnName("id")
               // .IsRequired();

            // prop para UserId
            builder.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();
            //.HasComment("El Usuario Id es requerido");
            //Este es comentario de base de dato crei que era para mandar el mansaje

            builder.HasOne(e => e.User)
            .WithOne() // uno a uno
            .HasForeignKey<UserEntity>(e => e.Id) // La clave 
            .OnDelete(DeleteBehavior.Cascade); // para que elimmine tambien el usuario asociado al cliente

            // Configuración para ClientTypeId
            builder.Property(e => e.ClientTypeId)
                .HasColumnName("client_type_id")
                .IsRequired();

            builder.HasOne(e => e.ClientType)
                .WithOne()
                .HasForeignKey<ClientTypeEntity>(e => e.Id)
                .OnDelete(DeleteBehavior.Restrict);

           
        }
    }
}
