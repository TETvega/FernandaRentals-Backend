using FernandaRentals.Database;
using FernandaRentals.Database.Entities;
using FernandaRentals.Helpers;
using FernandaRentals.Services.Interfaces;
using FernandaRentals.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FernandaRentals
{
    public class Startup
    {
        private readonly IConfiguration Configuration;

        // Creacion del Constructor de Startup
        public Startup(
            IConfiguration configuration
            )
        {
            this.Configuration = configuration;
        }

        // Configuracion de los servicios

        public void ConfigureServices(IServiceCollection services)
        {
            // agregando los Controladores
            services.AddControllers();
            services.AddEndpointsApiExplorer();

            // agregando Swagger al Proyecto
            services.AddSwaggerGen();

            // agregando el Http Contex
            // Utilizado para la validacion con identity en Audit 
            services.AddHttpContextAccessor();

            // agregando parte de la Configuracion de Conexion a la Base de datos
            var name = Configuration.GetConnectionString("DefaultConnection");

            //Agregando el DbContext
            services.AddDbContext<FernandaRentalsContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Agregando los Custom Services
            //services.AddTransient< INTERFAZ, SERVICIO>();
            services.AddTransient<IAuditService, AuditService>();


            // Agregando Identity 
            // Add Identity
            services.AddIdentity<UserEntity, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            }).AddEntityFrameworkStores<FernandaRentalsContext>()
              .AddDefaultTokenProviders();
            // agregando el servicio de Autentificacion mediante Token Bearer
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidAudience = Configuration["JWT:ValidAudience"],
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                    // agregando la configuracion del secreto o firma del Token 
                };
            });


            // Add AutoMapper
            services.AddAutoMapper(typeof(AutoMapperProfile));

            // CORS Configuration
            services.AddCors(opt =>
            {
                var allowURLS = Configuration.GetSection("AllowURLS").Get<string[]>();

                opt.AddPolicy("CorsPolicy", builder => builder
                .WithOrigins(allowURLS)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            });

        }

        // método se utiliza para configurar el pipeline de solicitud HTTP de la aplicación
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) // Configuracion del Middleware:
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("CorsPolicy");

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
