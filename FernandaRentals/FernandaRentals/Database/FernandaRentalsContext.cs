
using FernandaRentals.Database.Configuration;
using FernandaRentals.Database.Entities;
using FernandaRentals.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FernandaRentals.Database
{
    public class FernandaRentalsContext: IdentityDbContext<UserEntity>
    {
        //Variables Globales

        private readonly IAuditService _auditService;
        //Constructor de La Clase
        public FernandaRentalsContext(
            DbContextOptions options,
            IAuditService auditService
            ) : base(options)
        {
            this._auditService = auditService;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");


            // Creando Security Schema
            modelBuilder.HasDefaultSchema("security");

            modelBuilder.Entity<UserEntity>().ToTable("users");
            modelBuilder.Entity<IdentityRole>().ToTable("roles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("users_roles");

            //Estos son los permisos
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("users_claims");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("roles_claims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("users_logins");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("users_tokens");

            //Aplicacion de las Configuraciones de Entidades
            modelBuilder.ApplyConfiguration(new ClientConfiguration());
            modelBuilder.ApplyConfiguration(new CategoryProductConfiguration());
            modelBuilder.ApplyConfiguration(new ClienTypeConfiguration());
            modelBuilder.ApplyConfiguration(new DetailConfiguration());
            modelBuilder.ApplyConfiguration(new ProductConfiguration());
            modelBuilder.ApplyConfiguration(new EventConfiguration());
            modelBuilder.ApplyConfiguration(new NoteConfiguraction());
            modelBuilder.ApplyConfiguration(new ReservationConfiguration());



            // las configuraciones en decimales ahora se realizan en el archivo de Configuracion
            // fallo realizarlo alli
            modelBuilder.Entity<ClientTypeEntity>()
                 .Property(e => e.Discount)
                 .HasPrecision(18, 2);

            modelBuilder.Entity<DetailEntity>()
                .Property(e => e.Quantity)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EventEntity>()
                .Property(e => e.Discount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EventEntity>()
                .Property(e => e.EventCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EventEntity>()
                .Property(e => e.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductEntity>()
                .Property(e => e.Cost)
                .HasPrecision(18, 2);
            modelBuilder.Entity<DetailEntity>()
            .Property(d => d.UnitPrice)
            .HasColumnType("decimal(18,2)");
            // Ignorar la propiedad calculada TotalPrice
            modelBuilder.Entity<DetailEntity>()
            .Property(d => d.TotalPrice)
            .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ReservationEntity>()
                .Property(r => r.Count)
                .HasColumnType("decimal(18,2)"); // Ajusta la precisión y escala según tus necesidades

        }

        // Metodo para capturar el usuario que esta guardando los cambios creando o modificando 
        public override Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified
                ));

            foreach (var entry in entries)
            {
                var entity = entry.Entity as BaseEntity;
                if (entity != null)
                {
                    // si esta agregando o creando 
                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedBy = _auditService.GetUserId();
                        entity.CreatedDate = DateTime.Now;
                    }
                   // si esta modificando 
                    else
                    {
                        entity.UpdatedBy = _auditService.GetUserId();
                        entity.UpdatedDate = DateTime.Now;
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        // Agregando el contexto 

        public DbSet<CategoryProductEntity> CategoryProducts { get; set; }
        public DbSet<ClientTypeEntity> TypesOfClient { get; set; }
        public DbSet<DetailEntity> Details { get; set; }
        public DbSet<EventEntity> Events { get; set; }
        public DbSet<NoteEntity> Notes { get; set; }
        public DbSet<ProductEntity> Products { get; set; }
        public DbSet<ReservationEntity> Reservations { get; set; }
        public DbSet<ClientEntity> Clients { get; set; }

    }
}
