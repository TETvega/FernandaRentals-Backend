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
using FernandaRentals.Dtos.DashBoard.StadisticsDtos;

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
                        ClientEmail = _context.Users
                            .Where(u => u.Id == _context.Clients
                                .Where(c => c.Id == ev.ClientId)
                                .Select(c => c.UserId)
                                .FirstOrDefault())
                            .Select(u => u.Email)
                            .FirstOrDefault(),
                        Location = ev.Location
                        // Puedes agregar otras propiedades relacionadas si son necesarias
                    })
                    .OrderBy(dashboardEvent => dashboardEvent.StartDate) // Ordena por fecha más cercana
                    .ToListAsync();


                int totalUpcomingEvents = upcomingEventsDashboard.Count();


                //CALCULOS PARA LAS STADISTICAS --------------------------------------------------------------------------------------------------

                // el total remunerado 4 meses
                var grossProfit = await _context.Details
                                        .Where(d => d.CreatedDate >= currentDate.AddMonths(-4) && d.CreatedDate <= currentDate)
                                        .SumAsync(d => d.TotalPrice);

                // el valor neto de los ultimos 4 meses agrupado por mes y year 
                var NetProfitsAgruped = await _context.Details
                                    .Where(d => d.CreatedDate >= currentDate.AddMonths(-4) && d.CreatedDate <= currentDate)
                                    .GroupBy(d => new { Year = d.CreatedDate.Year, Month = d.CreatedDate.Month })
                                    .Select(group => new
                                    {
                                        Year = group.Key.Year,
                                        Month = group.Key.Month,
                                        Profit = group.Sum(d => d.TotalPrice)
                                    })
                                    .OrderBy(dto => dto.Year)
                                    .ThenBy(dto => dto.Month)
                                    .ToListAsync();

                // mapeo de las fechas para un resulado limpio para anner en el frontend
                var NetProfits = NetProfitsAgruped
                            .Select(data => new MonthlyProfitDto
                                 {
                                     Month = $"{data.Month:00}-{data.Year}", // Formato MM-YYYY
                                     Profit = data.Profit
                                 })
                             .ToList();

                // ingresos brutos de los ultimos 4 mes
                var BrutProfitsAgruped = await _context.Details
                                    .Where(d => d.CreatedDate >= currentDate.AddMonths(-4) && d.CreatedDate <= currentDate)
                                    // agrpar por mes y por año por si estamos en un nuevo , no cause problemas
                                    .GroupBy(d => new { Year = d.CreatedDate.Year, Month = d.CreatedDate.Month })
                                    // aqui se formatea para optener todo de una sola vez
                                    .Select(group => new 
                                    {
                                        Year = group.Key.Year,
                                        Month = group.Key.Month, 
                                        Profit = group.Sum(d => d.UnitPrice * d.Quantity )
                                    })
                                    .OrderBy(dto => dto.Month)
                                    .ToListAsync();
                // mapeo de las fechas
                var BrutProfits = BrutProfitsAgruped
                           .Select(data => new MonthlyProfitDto
                           {
                               Month = $"{data.Month:00}-{data.Year}", // Formato MM-YYYY
                               Profit = data.Profit
                           })
                            .ToList();


                // para el grafico en pastel 
                // el total de productos con su remuneracion totales de todos
                var productRevenues = await _context.Details
                                     .Where(d => d.CreatedDate >= currentDate.AddMonths(-4) && d.CreatedDate <= currentDate)
                                     .GroupBy(d => d.Product.Name)  // esta sera la llave 
                                     .Select(group => new ProductRevenueDto
                                          {
                                              Product = group.Key, // Nombre del producto 
                                              Revenue = group.Sum(d => d.TotalPrice) // Suma de los ingresos
                                           })
                                     .OrderByDescending(dto => dto.Revenue) // Ordenar por ingresos descendente para que sea mas acil el agregar a otros
                                     .ToListAsync();

                //Sacar solo los 10 mas importantes
                var topProducts = productRevenues.Take(10).ToList();

                var otherProductsRevenue = productRevenues.Skip(10)
                    .Any()? productRevenues.Skip(10).Sum(p => p.Revenue): 0;
                // Sumar los demas despues de los 10 primeros si es que hay mas 
                // y suma lo almacenado y si no pone en 0

                // si es que hay mas de 0 despues de los primeros 10 
                // se agrega otros con el total de las ri=umas de los restanmtes despues de los 10 
                if (otherProductsRevenue > 0)
                {
                    topProducts.Add(new ProductRevenueDto
                    {
                        Product = "Otros",
                        Revenue = otherProductsRevenue
                    });
                }
                

                var DasboardStadistics = new DashboardStatisticsDto
                {
                    GrossProfit = grossProfit,
                    GrossProfitByMonth = BrutProfits,
                    NetProfitByMonth = NetProfits,
                    ProductsRevenueDistribution = topProducts

                };
                // FIN DE LAS ESTADISTICAS


                //------------------------------- topProducts  DEL DASBOARD ------------------------------------

                // Calculo de los mas solicitado ultimo 4 meses
                var topRequestedProducts = await _context.Details
                    .Where(d => d.CreatedDate >= currentDate.AddMonths(-4) && d.CreatedDate <= currentDate) 
                    .GroupBy(d => d.Product.Name) 
                    .Select(group => new TopProductDto
                    {
                        Product = group.Key, 
                        Count = group.Sum(d => d.Quantity),
                        Revenue = group.Sum(d => d.TotalPrice)
                    })
                    .OrderByDescending(dto => dto.Count) 
                    .Take(3)
                    .ToListAsync();




                // productos menos solicitados 
                var leastRequestedProducts = await _context.Details
                    .Where(d => d.CreatedDate >= currentDate.AddMonths(-4) && d.CreatedDate <= currentDate) 
                    .GroupBy(d => d.Product.Name) 
                    .Select(group => new TopProductDto
                    {
                        Product = group.Key,
                        Count = group.Sum(d => d.Quantity),
                        Revenue = group.Sum(d => d.TotalPrice)
                    })
                    .OrderBy(dto => dto.Count) 
                    .Take(3) 
                    .ToListAsync();

                // productos con mas ingresos 
                var topRevenueProducts = await _context.Details
                    .Where(d => d.CreatedDate >= currentDate.AddMonths(-4) && d.CreatedDate <= currentDate) 
                    .GroupBy(d => d.Product.Name) 
                    .Select(group => new TopProductDto
                    {
                        Product = group.Key, 
                        Count = group.Sum(d => d.Quantity),
                        Revenue = group.Sum(d => d.TotalPrice),
                    })
                    .OrderByDescending(dto => dto.Revenue) 
                    .Take(3) 
                    .ToListAsync();

                var tops = new DashboardTopsDto 
                { 
                    TopRevenueProducts = topRevenueProducts,
                    LeastRequestedProducts = leastRequestedProducts,
                    TopRequestedProducts = topRequestedProducts,
                };
                // ----------------------------------->>>>>>>>>>>> TERMINA LOS TOPS 3 PRODUCTOS 

                // ------------------------------------------------------- COMPARACIONES UTILES 



                

                // Clientes
                // 7 dias 
                var clientsLast7Days = await _context.Clients
                    .Where(c => c.CreatedDate >= currentDate.AddDays(-7) && c.CreatedDate <= currentDate)
                    .CountAsync();
                // 14 dias atras
                var clientsPrevious7Days = await _context.Clients
                    .Where(c => c.CreatedDate >= currentDate.AddDays(-14) && c.CreatedDate < currentDate.AddDays(-7))
                    .CountAsync(); 


                // Productos
                // 7 dias
                var productsLast7Days = await _context.Products
                    .Where(p => p.CreatedDate >= currentDate.AddDays(-7) && p.CreatedDate <= currentDate)
                    .CountAsync(); 
                // 14 dias
                var productsPrevious7Days = await _context.Products
                    .Where(p => p.CreatedDate >= currentDate.AddDays(-14) && p.CreatedDate < currentDate.AddDays(-7))
                    .CountAsync();

                // eventos 
                // 7 dias atras
                var eventsLast7Days = await _context.Events
                    .Where(e => e.StartDate >= currentDate.AddDays(-7) && e.StartDate <= currentDate)
                    .CountAsync();

                // proximos 7 dias
                var eventsNext7Days = await _context.Events
                    .Where(e => e.StartDate > currentDate && e.StartDate <= currentDate.AddDays(7))
                    .CountAsync();


                // Llenar los objetos TComparation
                var ClientComparation = new TComparation
                {
                    NewTLast7Days= clientsLast7Days,
                    NewTPrevious7Days= clientsPrevious7Days,
                    PercentageChange = CalculatePercentageChange(clientsLast7Days, clientsPrevious7Days),
                    Message = MessageChange(clientsLast7Days, clientsPrevious7Days)
                };

                var ProductsComparation = new TComparation
                {
                    NewTLast7Days = productsLast7Days,
                    NewTPrevious7Days = productsPrevious7Days,
                    PercentageChange = CalculatePercentageChange(productsLast7Days, productsPrevious7Days),
                    Message = MessageChange(productsLast7Days, productsPrevious7Days)
                };

                var EventsComparation = new TComparation
                {
                    NewTLast7Days = eventsNext7Days,
                    NewTPrevious7Days = eventsLast7Days,
                    PercentageChange = CalculatePercentageChange(eventsNext7Days, eventsLast7Days),
                    Message = MessageChange(eventsNext7Days, eventsLast7Days)
                };

                var comparation = new DashboardComparationDto
                {
                    ClientComparation = ClientComparation,
                    ProductsComparation = ProductsComparation,
                    EventsComparation = EventsComparation
                };
                // ------------------------------------ FIN DE LAS COMPARACIONES


                var dashboardDto = new DashBoardDto
                {
                    TotalProducts = totalProducts,
                    TotalClients = totalClients,
                    TotalUpcomingEvents = totalUpcomingEvents,
                    UpcomingEvents = upcomingEventsDashboard,
                    Statistics = DasboardStadistics,
                    Tops = tops,
                    Comparisons = comparation,
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

        public async Task<ResponseDto<FinancialReportDto>> GetMonthlyFinancialReport(MonthDataDto monthData)
        {
           
            DateTime startOfMonth = new DateTime(monthData.Year, monthData.Month, 1); // primer día del mes
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1); // último día del mes

            try
            {
               // eventos del mes seleccionado
                var events = await _context.Events
                    .Where(e => e.StartDate >= startOfMonth && e.EndDate <= endOfMonth)
                    .ToListAsync();

                if (events.Count == 0) return ResponseHelper.ResponseError<FinancialReportDto>(404, $"No hay eventos para el mes de {monthData.MonthName} {monthData.Year}.");
                

                // total de dinero
                var totalRevenue = await _context.Events
                    .Where(e => e.StartDate >= startOfMonth && e.EndDate <= endOfMonth)
                    .SumAsync(e => e.Total);

                var totalDiscounts = await _context.Events
                    .Where(e => e.StartDate >= startOfMonth && e.EndDate <= endOfMonth)
                    .SumAsync(e => e.Discount);

                var eventCount = await _context.Events
                    .Where(e => e.StartDate >= startOfMonth && e.EndDate <= endOfMonth)
                    .CountAsync();

                // promedio de los ingresos
                var averageRevenue = eventCount > 0 ? totalRevenue / eventCount : 0;

                // Crear el DTO de reporte financiero
                var report = new FinancialReportDto
                {
                    TotalRevenue = totalRevenue,
                    TotalDiscounts = totalDiscounts,
                    EventCount = eventCount,
                    AverageRevenue = Math.Round(averageRevenue, 2, MidpointRounding.ToEven)
                };

                return ResponseHelper.ResponseSuccess<FinancialReportDto>(200, $"Reporte del mes de {monthData.MonthName} {monthData.Year} recibido correctamente.", report);
            }
            catch (Exception e)
            {
                // Manejo de errores
                _logger.LogError(e, "Error al hacer la consulta en el try.");
                return ResponseHelper.ResponseError<FinancialReportDto>(500, "Se produjo un error al obtener los datos.");
            }
        }


        public decimal CalculatePercentageChange(int currentValue, int previous)
        {
            if (previous == 0) return 0;

            var change = Math.Round(((decimal)(currentValue - previous) / previous) * 100, 5);
            return change;
        }

        public string MessageChange(int currentValue, int previous)

        {
            var Message = "";

            if (previous == 0)
            {
                Message = $"infinite%";
            };

            if (previous == currentValue)
            {
                Message = $"Sin Cambios";
            };



            if (previous > currentValue) {
                Message = $"{Message}% Disminucion";
            };

            if (previous < currentValue)
            {
                Message = $"{Message}% Aumento";
            }
            return Message;
        }
    }
}
