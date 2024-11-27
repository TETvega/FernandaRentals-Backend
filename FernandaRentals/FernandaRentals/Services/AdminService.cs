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
using FernandaRentals.Dtos.Admin;

namespace FernandaRentals.Services
{
    public class AdminService: IAdminService
    {
        private readonly FernandaRentalsContext _context;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IAuditService _auditService;
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<IEventService> _logger;
        public AdminService(
            FernandaRentalsContext context,IMapper mapper,
            IAuthService authService,IAuditService auditService,
            ILogger<IEventService> logger,
            UserManager<UserEntity> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            _context = context;
            _mapper = mapper;
            _authService = authService;
            _auditService = auditService;
            _userManager = userManager;
            _roleManager = roleManager;
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

        public async Task<ResponseDto<List<ClientsDataDto>>> GetClientsData()
        {
            var usersInClientRole = await _userManager.GetUsersInRoleAsync(RolesConstants.CLIENT); // queda en IList

            if (usersInClientRole == null || !usersInClientRole.Any()) return ResponseHelper.ResponseError<List<ClientsDataDto>>(404, $"No se encontraron a los clientes.");

            var usersInClientRoleList = usersInClientRole.ToList(); // se pasa a List

            var clientsData = new List<ClientsDataDto>();

            foreach (var user in usersInClientRoleList)
            {
                // Obtener el cliente asociado al usuario
                var client = await _context.Clients
                    .Include(c => c.ClientType)
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);

                if (client == null)
                {
                    continue; // Si no hay cliente asociado, omitir este usuario
                }

                var totalPastEvents = await _context.Events
                    .CountAsync(e => e.ClientId == client.Id && e.EndDate < DateTime.Now);

                var totalUpcomingEvents = await _context.Events
                    .CountAsync(e => e.ClientId == client.Id && e.StartDate >= DateTime.Now);

                clientsData.Add(new ClientsDataDto
                {
                    ClientId = client.Id,
                    ClientName = user.Name,
                    ClientEmail = user.Email,
                    ClientTypeName = client.ClientType.Name,
                    ClientTypeId = client.ClientType.Id,
                    TotalPastEvents = totalPastEvents,
                    TotalUpcomingEvents = totalUpcomingEvents
                });
            }

            var sortedClients = clientsData.OrderByDescending(c => c.TotalPastEvents + c.TotalUpcomingEvents).ToList();


            return ResponseHelper.ResponseSuccess<List<ClientsDataDto>>(200, $"{MessagesConstant.RECORDS_FOUND}", sortedClients);
        }


    }
}
