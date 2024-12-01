using AutoMapper;
using FernandaRentals.Constants;
using FernandaRentals.Database;
using FernandaRentals.Database.Entities;
using FernandaRentals.Dtos.Common;
using FernandaRentals.Dtos.Events;
using FernandaRentals.Dtos.Events.Helper_Dto;
using FernandaRentals.Dtos.Notes;
using FernandaRentals.Dtos.Products;
using FernandaRentals.Services.Interfaces;
using InmobiliariaUNAH.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FernandaRentals.Services
{
    public class EventsService : IEventService
    {

        private readonly FernandaRentalsContext _context;
        private readonly IMapper _mapper;
        private readonly IAuthService _authService;
        private readonly IAuditService _auditService;
        private readonly UserManager<UserEntity> _userManager;
        private readonly ILogger<IEventService> _logger;

        public EventsService(
            FernandaRentalsContext context,
            IMapper mapper,
            IAuthService authService,
            IAuditService auditService,
            UserManager<UserEntity> userManager,
            ILogger<IEventService> logger)
        {
            _context = context;
            _mapper = mapper;
            _authService = authService;
            _auditService = auditService;
            _userManager = userManager;
            _logger = logger;
        }
        public async Task<ResponseDto<List<EventDto>>> GetAllEventsAsync()
        {
            var eventsEntity = await _context.Events
                .Include(e => e.Client)
                    .ThenInclude(c => c.User)
                .Include(e => e.Client)
                    .ThenInclude(c => c.ClientType)
                .Include(e => e.EventDetails)
                    .ThenInclude(ed => ed.Product)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            var eventsDto = _mapper.Map<List<EventDto>>(eventsEntity);

            return new ResponseDto<List<EventDto>>
            {
                StatusCode = 200,
                Status = true,
                Message = "Listado de eventos obtenidos correctamente",
                Data = eventsDto
            };
        }


        // TODO: New Method: GetAllEventsByUserIdAsync
        public async Task<ResponseDto<List<EventDto>>> GetAllEventsByUserIdAsync()
        {
            var userId = _auditService.GetUserId();
            if (userId is null) return ResponseHelper.ResponseError<List<EventDto>>(404, "No se encontró el id del usuario.");

            var userEntity = await _userManager.FindByIdAsync(userId);
            if (userEntity is null) return ResponseHelper.ResponseError<List<EventDto>>(404, "No se encontró el usuario.");

            var clientEntity = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId);
            if (clientEntity is null) return ResponseHelper.ResponseError<List<EventDto>>(404, "El cliente no ha sido encontrado.");

            var eventsEntity = await _context.Events
                .Include(e => e.Client)
                .Include(e => e.EventDetails)
                    .ThenInclude(ed => ed.Product)
                .Where(e => e.ClientId == clientEntity.Id)
                .ToListAsync();

            var eventsDto = _mapper.Map<List<EventDto>>(eventsEntity);

            return new ResponseDto<List<EventDto>>
            {
                StatusCode = 200,
                Status = true,
                Message = $"Listado de eventos de {userEntity.Name}obtenida correctamente",
                Data = eventsDto
            };
        }

        public async Task<ResponseDto<EventDto>> GetEventById(Guid id)
        {
            var eventEntity = await _context.Events
            .Include(e => e.Client)
            .Include(e => e.EventDetails)
            .ThenInclude(ed => ed.Product)
            .FirstOrDefaultAsync(ev => ev.Id == id);

            if (eventEntity == null) return ResponseHelper.ResponseError<EventDto>(404, "No se encontró el evento");

            var eventDto = _mapper.Map<EventDto>(eventEntity);
            var clientEntity = await _context.Clients.FirstOrDefaultAsync(c => c.Id == eventEntity.ClientId);
            eventDto.UserId = Guid.Parse(clientEntity.UserId);

            return ResponseHelper.ResponseSuccess<EventDto>(200, "Listado de eventos obtenida correctamente", eventDto);
        }

        public async Task<ResponseDto<EventDto>> CreateEvent(EventCreateDto dto)
        {
            var userId = _auditService.GetUserId();
            if (userId == null)
            {
                return new ResponseDto<EventDto>
                {
                    StatusCode = 404,
                    Status = true,
                    Message = "Error al obtener Id del usuario"
                };
            }
            var userEntity = await _userManager.FindByIdAsync(userId);
            if (userEntity is null)
            {
                return new ResponseDto<EventDto>
                {
                    StatusCode = 404,
                    Status = true,
                    Message = "Error al recuperar usuario"
                };
            }

            var clientEntity = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == userId); // TODO: Agregar validaciones de existencia
            var clientTypeEntity = await _context.TypesOfClient.FindAsync(clientEntity.ClientTypeId); // id cliente

            var eventEntity = _mapper.Map<EventEntity>(dto);
            eventEntity.ClientId = clientEntity.Id;

            ///// validacion si existen los productos
            var existingProducts = await _context.Products.ToListAsync();
            var productIdsInDto = dto.Productos.Select(p => p.ProductId).ToList();

            // anner esta es para ver cuales no existen 
            // el ANY es "si alguno de los de la tabla existen" entoncess es un si , el ! es si al menos uno de ellos NO existe en la tabla
            //Productos que están presentes en el DTO pero no existen en la base de datos.
            var ProductsNoExistentes = productIdsInDto
                .Where(dtoProductId => !existingProducts.Any(eP => eP.Id == dtoProductId))
                .ToList();

            // si hay un producto existente sera 1 o mayor a 1 
            var ProductosNoExistentesdelDto = ProductsNoExistentes.Count();

            if (ProductosNoExistentesdelDto > 0)
            {
                return ExeptionProductosNoExistentes(ProductsNoExistentes);

            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {

                try
                {

                    await _context.Events.AddAsync(eventEntity);
                    await _context.SaveChangesAsync();
                    // verficacion de fechas 
                    // Todas las Reservaciones que coinciden con los id de productos existentes
                    // Se obtienen todas las reservas que coinciden con los IDs de productos proporcionados en la solicitud. Para verificar las reservas existentes que puedan afectar el stock de los productos.
                    var ExistinReservations = await _context.Reservations
                        .Where(reservation => productIdsInDto.Contains(reservation.ProductId))
                        .ToListAsync();

                    DateTime startDate = dto.StartDate.Date;
                    DateTime endDate = dto.EndDate.Date;

                    var validacionFechasResult = ValidarFechas(startDate, endDate);
                    if (validacionFechasResult != null)
                    {
                        return validacionFechasResult;
                    }

                    var newReservations = new List<ReservationEntity>();
                    var errorMesagesValidacionProductos = await ValidacionDeProductosFechas(startDate, endDate, dto.Productos, newReservations, ExistinReservations, eventEntity, existingProducts);
                    if (errorMesagesValidacionProductos.Length > 0)
                        return new ResponseDto<EventDto>
                        {
                            StatusCode = 405,
                            Status = false,
                            Message = errorMesagesValidacionProductos,
                        };


                    // guardar todos los cambios 
                    await _context.Reservations.AddRangeAsync(newReservations);
                    await _context.SaveChangesAsync();

                    // para obtener los detalles y el costo del evento 
                    // mira esta chulada encontre algo en tik tok y busque y fua gloria oro puro 
                    // https://es.stackoverflow.com/questions/460803/se-pueden-devolver-varios-valores-con-un-return-c
                    // Detro de la funcion estoy definiendo el tipo de dato osea returna el tipo de dato por dentro
                    var (newListDetails, eventCost) = GenerarDetallesYCalcularCosto(dto, eventEntity, existingProducts);

                    await _context.Details.AddRangeAsync(newListDetails);
                    await _context.SaveChangesAsync();

                    // creando el evento correctamente 
                    eventEntity.EventCost = eventCost;

                    // TODO EL DESCUENTO NECESITAMOS SABER QUIEN ES EL QUE MANDA ESTO 


                    var discount = eventEntity.Discount = ((eventEntity.EventCost) * (clientTypeEntity.Discount));
                    eventEntity.Total = eventEntity.EventCost - discount;


                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var eventDto = _mapper.Map<EventDto>(eventEntity);
                    eventDto.UserId = Guid.Parse(userEntity.Id);
                    return new ResponseDto<EventDto>
                    {
                        StatusCode = 200,
                        Status = true,
                        Message = "Exito al CREAR UN EVENTO",
                        Data = eventDto
                    };

                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(e, "Error al crear un Evento en el try");
                    return new ResponseDto<EventDto>
                    {
                        StatusCode = 500,
                        Status = false,
                        Message = "Error al crear un nuevo Evento"
                    };

                }

            }
        }

        public async Task<ResponseDto<EventDto>> EditEventAsync(EventEditDto dto, Guid id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var CheckEventEntity = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
                if (CheckEventEntity is null)
                    return ResponseHelper.ResponseError<EventDto>(404, "El evento a editar no fue encontrado.");// Se creó esta funcion para ahorrar lineas de codigo. TODO: Aplicar en demás servicios.            

                var userIdFromAuditService = _auditService.GetUserId();
                if (userIdFromAuditService == null) return ResponseHelper.ResponseError<EventDto>(404, "Error al obtener Id del usuario.");

                var userEntity = await _userManager.FindByIdAsync(userIdFromAuditService); // extraer userEntity apartir del id que retorna GetUserId()
                if (userEntity is null) return ResponseHelper.ResponseError<EventDto>(404, "Error al recuperar usuario de la base de datos.");


                var userRoles = await _userManager.GetRolesAsync(userEntity); // Obtener roles de userEntity

                var clientEntity = await _context.Clients.FirstOrDefaultAsync(c => c.Id == CheckEventEntity.ClientId); // buscar clientEntity para usar el UserId
                if (clientEntity is null) return ResponseHelper.ResponseError<EventDto>(404, "Cliente asociado al evento no encontrado.");

                if ((!userRoles.Contains(RolesConstants.ADMIN)) && (userIdFromAuditService != clientEntity.UserId))  // si el usuario que está haciendo la peticion no es rol ADMIN & no es el Cliente que creo el evento, return.
                    return ResponseHelper.ResponseError<EventDto>(404, "No estás autorizado para editar o cancelar este evento.");
                // Termina validacion Inicial.

                var clientTypeEntity = await _context.TypesOfClient.FindAsync(clientEntity.ClientTypeId);
                if (clientTypeEntity is null) return ResponseHelper.ResponseError<EventDto>(404, "Este evento no te pertenece. Solo puedes editar o cancelar tus eventos."); // !! Suponiendo que el usuaerio ADMIN no está vinculado a un Cliente

                DateTime startDate = dto.StartDate.Date; DateTime endDate = dto.EndDate.Date;
                if (ValidarFechas(startDate, endDate) != null) return ValidarFechas(startDate, endDate);

                var existingProducts = await _context.Products.ToListAsync();
                var productIdsInDto = dto.Productos.Select(p => p.ProductId).ToList();

                var ProductsNoExistentes = productIdsInDto
                   .Where(dtoProductId => !existingProducts.Any(eP => eP.Id == dtoProductId))
                   .ToList();


                // Compartido
                var ProductosNoExistentesdelDto = ProductsNoExistentes.Count();
                if (ProductosNoExistentesdelDto > 0) return ExeptionProductosNoExistentes(ProductsNoExistentes);
                ///
                try
                {
                    var eventEntity = _mapper.Map(dto, CheckEventEntity); // Actualiza el evento existente                                                         
                    eventEntity.ClientId = clientEntity.Id;

                    _context.Events.Update(eventEntity);
                    await _context.SaveChangesAsync();

                    var details = await _context.Details.Where(d => d.EventId == id).ToListAsync();
                    _context.Details.RemoveRange(details);
                    await _context.SaveChangesAsync();

                    var reservations = await _context.Reservations.Where(d => d.EventId == id).ToListAsync();
                    _context.Reservations.RemoveRange(reservations);
                    await _context.SaveChangesAsync();

                    var ExistinReservations = await _context.Reservations
                        .Where(reservation => productIdsInDto.Contains(reservation.ProductId))
                        .ToListAsync();

                    var newReservations = new List<ReservationEntity>();
                    var errorMesagesValidacionProductos = await ValidacionDeProductosFechas(startDate, endDate, dto.Productos, newReservations, ExistinReservations, eventEntity, existingProducts);
                    if (errorMesagesValidacionProductos.Length > 0) return ResponseHelper.ResponseError<EventDto>(405, errorMesagesValidacionProductos);


                    await _context.Reservations.AddRangeAsync(newReservations);
                    await _context.SaveChangesAsync();

                    var (newListDetails, eventCost) = GenerarDetallesYCalcularCosto(dto, eventEntity, existingProducts);


                    await _context.Details.AddRangeAsync(newListDetails);
                    await _context.SaveChangesAsync();

                    eventEntity.EventCost = eventCost;


                    var discount = eventEntity.Discount = ((eventEntity.EventCost) * (clientTypeEntity.Discount));
                    eventEntity.Total = eventEntity.EventCost - discount;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    var eventDto = _mapper.Map<EventDto>(eventEntity);
                    return new ResponseDto<EventDto>
                    {
                        StatusCode = 200,
                        Status = true,
                        Message = "Exito al EDITAR UN EVENTO",
                        Data = eventDto
                    };
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(e, "Error al editar el evento en el try.");
                    return ResponseHelper.ResponseError<EventDto>(500, "Error al editar el evento.");
                }
            }
        }


        public async Task<ResponseDto<EventDto>> CancelEventAsync(Guid id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var eventEntity = await _context.Events.FindAsync(id);
                    if (eventEntity is null) return ResponseHelper.ResponseError<EventDto>(404, $"No se encontró el Evento con el id: {id}");

                    var userIdFromAuditService = _auditService.GetUserId();
                    if (userIdFromAuditService == null) return ResponseHelper.ResponseError<EventDto>(404, "Error al obtener Id del usuario.");

                    var userEntity = await _userManager.FindByIdAsync(userIdFromAuditService); // extraer userEntity apartir del id que retorna GetUserId()
                    if (userEntity is null) return ResponseHelper.ResponseError<EventDto>(404, "Error al recuperar usuario de la base de datos.");


                    var userRoles = await _userManager.GetRolesAsync(userEntity); // Obtener roles de userEntity

                    var clientEntity = await _context.Clients.FirstOrDefaultAsync(c => c.Id == eventEntity.ClientId); // buscar clientEntity para usar el UserId
                    if (clientEntity is null) return ResponseHelper.ResponseError<EventDto>(404, "Cliente asociado al evento no encontrado.");

                    if ((!userRoles.Contains(RolesConstants.ADMIN)) && (userIdFromAuditService != clientEntity.UserId))  // si el usuario que está haciendo la peticion no es rol ADMIN & no es el Cliente que creo el evento, return.
                        return ResponseHelper.ResponseError<EventDto>(401, "No estás autorizado para editar o cancelar este evento.");

                    _context.Events.Remove(eventEntity);
                    await _context.SaveChangesAsync();

                    var details = await _context.Details.Where(d => d.EventId == id).ToListAsync();
                    _context.Details.RemoveRange(details);
                    await _context.SaveChangesAsync();

                    var reservations = await _context.Reservations.Where(d => d.EventId == id).ToListAsync();
                    _context.Reservations.RemoveRange(reservations);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new ResponseDto<EventDto>
                    {
                        StatusCode = 200,
                        Status = true,
                        Message = "El evento ha sido cancelado correctamente"
                    };
                }
                catch (Exception e)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(e, "Error al cancelar evento en el try.");
                    return new ResponseDto<EventDto>
                    {
                        StatusCode = 500,
                        Status = false,
                        Message = "Error al cancelar evento."
                    };
                }
            }
        }


        private async Task<string> ValidacionDeProductosFechas(
          DateTime startDate,
          DateTime endDate,
          IEnumerable<EventProducDto> productos,
          List<ReservationEntity> newReservations,
          IEnumerable<ReservationEntity> ExistinReservations,
          EventEntity eventEntity,
          List<ProductEntity> existingProducts)
        {
            var errorMessages2 = new StringBuilder();
            var productosData = await _context.Products.ToListAsync();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                foreach (var product in productos)
                {
                    var productId = product.ProductId;
                    var CantidadSolicitada = product.Quantity;

                    // para calcular la cantidad de producto en una fecha especifica
                    var existingTotalCount = ExistinReservations
                        .Where(reservation => reservation.ProductId == productId && reservation.Date == date)
                        .Sum(reservation => reservation.Count);

                    var productEntityIteracion = existingProducts.FirstOrDefault(p => p.Id == productId);
                    var StockProductoIteracion = productEntityIteracion?.Stock ?? 0;

                    // verificacion de los de reserva contra el stock
                    if (existingTotalCount + CantidadSolicitada <= StockProductoIteracion)
                    {
                        newReservations.Add(new ReservationEntity
                        {
                            ProductId = productId,
                            EventId = eventEntity.Id,
                            Date = date,
                            Count = CantidadSolicitada,
                            Name = eventEntity.Name,
                        });
                    }
                    else
                    {
                        var productoEnData = productosData.FirstOrDefault(p => p.Id == productId);
                        errorMessages2.AppendLine($"El producto {productoEnData.Name} no tiene suficiente stock para la fecha {date.ToShortDateString()}.");
                    }
                }
            }

            // Si hay error los devuelve en cadena de string y si no un string vacio asi ""
            return errorMessages2.Length > 0 ? errorMessages2.ToString().Replace("\r\n", " ") : string.Empty;
        }
        // Funcion parala validacion de las Fechas Aqui va toda la Loguica conrespecto a Fechas
        private ResponseDto<EventDto> ValidarFechas(DateTime startDate, DateTime endDate)
        {
            if (startDate.Date <= DateTime.Today)
            {
                return new ResponseDto<EventDto>
                {
                    StatusCode = 400,
                    Status = false,
                    Message = "La fecha de inicio no puede ser el dia de hoy, ni dias anteriores. Por favor, seleccione una fecha a partir de mañana."
                };
            }
            if (startDate > endDate)
            {
                return new ResponseDto<EventDto>
                {
                    StatusCode = 400,
                    Status = false,
                    Message = "La fecha de Inicio no puede ser despúes de la Fecha de Finalización. Por favor, seleccione una fecha a partir de mañana."
                };
            }


            return null;
        }


        private (List<DetailEntity> details, decimal eventCost) GenerarDetallesYCalcularCosto(
            EventCreateDto dto,
            EventEntity eventEntity,
            List<ProductEntity> existingProducts)
        {
            //lista de detalles del evento
            var newListDetails = new List<DetailEntity>();
            decimal eventCost = 0; // costo final del evento en cuestion

            foreach (var product in dto.Productos)
            {
                var productoIteracion = existingProducts.FirstOrDefault(p => p.Id == product.ProductId);

                if (productoIteracion != null)
                {
                    newListDetails.Add(new DetailEntity
                    {
                        EventId = eventEntity.Id,
                        ProductId = product.ProductId,
                        Quantity = product.Quantity,
                        UnitPrice = productoIteracion.Cost,
                        TotalPrice = product.Quantity * productoIteracion.Cost,
                    });

                    eventCost += product.Quantity * productoIteracion.Cost;
                }
            }

            return (newListDetails, eventCost);
        }


        private ResponseDto<EventDto> ExeptionProductosNoExistentes(List<Guid> ProductsNoExistentes)
        {
            // https://www.youtube.com/watch?v=ZAgnc0sbzA8&ab_channel=ConsejosC%23
            // el video habla por si le queres entender como funciona lo de concatenacion de strigs
            // aunque aun asi no encuentro como hacer que se pongan con una linea de por medio sale /r/n
            var errorMessages = new StringBuilder();
            foreach (var productId in ProductsNoExistentes)
            {
                errorMessages.AppendLine($"{productId}");
            }
            var errorMessagesString = errorMessages.ToString().Replace("\r\n", ", ");

            return new ResponseDto<EventDto>
            {
                StatusCode = 404,
                Status = false,
                Message = $"El o los productos: {errorMessagesString}no exiten en la base de datos."
            };
        }


    }
}
