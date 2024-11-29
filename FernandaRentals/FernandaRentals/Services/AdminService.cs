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
using FernandaRentals.Dtos.Auth;

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
                    continue; // sino hay cliente asociado, omitir este usuario
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
        public async Task<ResponseDto<List<UserDto>>> GetAdminUsers()
        {
            var usersEntity = await _userManager.Users.ToListAsync();

            if (usersEntity == null || !usersEntity.Any()) return ResponseHelper.ResponseError<List<UserDto>>(404, $"No se encontraron a los usuarios.");

            var adminUsers = new List<UserDto>();

            foreach (var user in usersEntity)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains($"{RolesConstants.ADMIN}"))
                {
                    adminUsers.Add(new UserDto
                    {
                        UserId = user.Id,
                        UserName = user.Name,
                        UserEmail = user.Email,
                        UserRole = $"{RolesConstants.ADMIN}"
                    });
                }
            }

            if(adminUsers.Count() == 0) return ResponseHelper.ResponseError<List<UserDto>>(404, $"No se encontraron a los usuarios admin.");

            return ResponseHelper.ResponseSuccess<List<UserDto>>(200, $"{MessagesConstant.RECORDS_FOUND}", adminUsers);

        }

        public async Task<ResponseDto<UserAdminCreateDto>> CreateAdminUser(UserAdminCreateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userEntity = new UserEntity
                {
                    Email = dto.Email,
                    UserName = dto.Email,
                    Name = dto.Name
                };

                var createResult = await _userManager.CreateAsync(userEntity, dto.Password);
                if (!createResult.Succeeded)
                {
                    List<IdentityError> errorList = createResult.Errors.ToList();  // Listamos los errores
                    string errors = "";

                    foreach (var error in errorList)
                    {
                        errors += error.Description;

                        // si el error trata de DuplicateUserName, personalizar ErrorMessage
                        if (error.Code == "DuplicateUserName") return ResponseHelper.ResponseError<UserAdminCreateDto>(400, "El email ya está registrado en el sistema.");
                    }
                }

                var roleResult = await _userManager.AddToRoleAsync(userEntity, RolesConstants.ADMIN);
                if (!roleResult.Succeeded) return ResponseHelper.ResponseError<UserAdminCreateDto>(400, $"Error a asignar rol: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");

                await transaction.CommitAsync();

                var userAdmin = new UserAdminCreateDto
                {
                    Name = userEntity.Name,
                    Email = userEntity.Email,
                    Password = dto.Password
                };

                return ResponseHelper.ResponseSuccess<UserAdminCreateDto>(201, $"{MessagesConstant.CREATE_SUCCESS}", userAdmin);
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync(); // Rollback si ocurre una excepción
                _logger.LogError(e, "Ocurrió un error inesperado al registrar el usuario.");
                return ResponseHelper.ResponseError<UserAdminCreateDto>(400, "Ocurrió un error inesperado al registrar el usuario.");

            }
        }

        public async Task<ResponseDto<UserAdminEditDto>>EditAdminUser(UserAdminEditDto dto, string id)
        {
            var userEntity = await _userManager.FindByIdAsync(id);
            if(userEntity is null) return ResponseHelper.ResponseError<UserAdminEditDto>(404, $"No se encontró al usuario {dto.Name}.");


            userEntity.Email = dto.Email;
            userEntity.UserName = dto.Email;
            userEntity.Name = dto.Name;

            userEntity.NormalizedEmail = dto.Email.ToUpper();
            userEntity.NormalizedUserName = dto.Email.ToUpper();

            await _userManager.UpdateAsync(userEntity);

            return ResponseHelper.ResponseSuccess<UserAdminEditDto>(200, $"{MessagesConstant.UPDATE_SUCCESS}");
        }
    }
}
