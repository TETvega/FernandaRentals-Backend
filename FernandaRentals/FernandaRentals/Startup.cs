﻿using FernandaRentals.Database;
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
        private IConfiguration Configuration { get; }

        // Creacion del Constructor de Startup
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // Configuracion de los servicios

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(); // agregando los Controladores
            services.AddEndpointsApiExplorer();         
            services.AddSwaggerGen(); // agregando Swagger al Proyecto

            services.AddControllers().AddNewtonsoftJson(options => // Añadir Controladores con Newtonsoft.Json (del pack: Microsoft.AspNetCore.Mvc.NewtonsoftJson)
            {
                options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore; // Esto le indica a Newtonsoft.Json que ignore las referencias cíclicas durante la serialización.
            });

            //Agregando el DbContext
            services.AddDbContext<FernandaRentalsContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Add custom services
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<ICategoryProductService, CategoryProductService>();
            services.AddTransient<INoteService, NotesService>();
            services.AddTransient<IClientTypeService, ClientTypeService>();
            services.AddTransient<IEventService, EventsService>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IAuditService, AuditService>();
            services.AddTransient<IAdminService, AdminService>();
            services.AddTransient<IClientService, ClientService>();


            // Add Identity
            services.AddIdentity<UserEntity, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            }).AddEntityFrameworkStores<FernandaRentalsContext>()
              .AddDefaultTokenProviders();

            // Registrar TokenValidationParameters como Singleton
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidAudience = Configuration["JWT:ValidAudience"],
                ValidIssuer = Configuration["JWT:ValidIssuer"],
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
            };
            services.AddSingleton(tokenValidationParameters);

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
                options.TokenValidationParameters = tokenValidationParameters;
                    // agregando la configuracion del secreto o firma del Token 
            });

            services.AddAuthorization();
            // agregando el Http Contex
            // Utilizado para la validacion con identity en Audit 
            services.AddHttpContextAccessor();

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
