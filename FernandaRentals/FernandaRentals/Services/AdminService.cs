using AutoMapper;
using FernandaRentals.Database.Entities;
using FernandaRentals.Database;
using FernandaRentals.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using FernandaRentals.Dtos.Common;
using FernandaRentals.Dtos.DashBoard;
using Microsoft.EntityFrameworkCore;
using FernandaRentals.Constants;
using FernandaRentals.Dtos.Events;
using InmobiliariaUNAH.Helpers;

namespace FernandaRentals.Services
{
    public class AdminService: IAdminService
    {
        private readonly FernandaRentalsContext _context;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IAuditService _auditService;
        private readonly UserManager<UserEntity> _userManager;
        private readonly ILogger<IEventService> _logger;
        public AdminService(
             FernandaRentalsContext context,IMapper mapper,
            IAuthService authService,IAuditService auditService,
            UserManager<UserEntity> userManager,
            ILogger<IEventService> logger
            )
        {
            _context = context;
            _mapper = mapper;
            _authService = authService;
            _auditService = auditService;
            _userManager = userManager;
            _logger = logger;
        }


        public async Task<ResponseDto<DashBoardDto>> GetDashBoardData()
        {
            try
            {
                int totalProducts = await _context.Products.CountAsync();
                int totalClients = await _context.Clients.CountAsync();

                var currentDate = DateTime.Now;
                var twoWeeksFromNow = currentDate.AddDays(14);

                var upcomingEventsDashboard = await _context.Events
                    .Where(ev => ev.StartDate > currentDate && ev.StartDate <= twoWeeksFromNow) // eventos a realizarse en las próximas 2 semanas
                    .Select(ev => new DashboardUpcomingEvents
                    {
                        Id = ev.Id,
                        Name = ev.Name,
                        StartDate = ev.StartDate,
                        EndDate = ev.EndDate,
                        ClientName = _context.Users
                            .Where(u => u.Id == _context.Clients
                                .Where(c => c.Id == ev.ClientId)
                                .Select(c => c.UserId)
                                .FirstOrDefault())
                            .Select(u => u.Name)
                            .FirstOrDefault(),
                        // Puedes agregar otras propiedades relacionadas si son necesarias
                    })
                    .OrderBy(dashboardEvent => dashboardEvent.StartDate) // Ordena por fecha más cercana
    .ToListAsync();


                int totalUpcomingEvents = upcomingEventsDashboard.Count();


                var dashboardDto = new DashBoardDto
                {
                    TotalProducts = totalProducts,
                    TotalClients = totalClients,
                    TotalUpcomingEvents = totalUpcomingEvents,
                    UpcomingEvents = upcomingEventsDashboard
                };
                return ResponseHelper.ResponseSuccess<DashBoardDto>(200, $"{MessagesConstant.RECORDS_FOUND}", dashboardDto);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error al hacer la consulta en el try.");
                return ResponseHelper.ResponseError<DashBoardDto>(500, $"Se produjo un error al obtener los datos.");

            }

        }



    }
}
