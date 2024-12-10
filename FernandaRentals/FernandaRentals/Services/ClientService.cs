using AutoMapper;
using FernandaRentals.Database.Entities;
using FernandaRentals.Database;
using FernandaRentals.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using FernandaRentals.Dtos.Common;
using FernandaRentals.Dtos.Client;
using Microsoft.EntityFrameworkCore;
using FernandaRentals.Dtos.Admin;
using InmobiliariaUNAH.Helpers;
using FernandaRentals.Constants;

namespace FernandaRentals.Services
{
    public class ClientService : IClientService
    {
        private readonly FernandaRentalsContext _context;
        private readonly ILogger<IEventService> _logger;
        private readonly UserManager<UserEntity> _userManager;

        public ClientService(
            FernandaRentalsContext context,
            IAuthService authService, 
            IAuditService auditService,
            ILogger<IEventService> logger,
            UserManager<UserEntity> userManager
            )
        {
            this._context = context;
            this._logger = logger;
            this._userManager = userManager;
        }

        public async Task<ResponseDto<ClientEditDto>> EditClient(ClientEditDto dto, Guid id)
        {
            // id = client id
            // dto = ClientName ,  ClientTypeId  <-- datos a editar
            var clientEntity = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == id);

            // validar que el nuevo typeClient exista
            var clientTypeExists = await _context.TypesOfClient.AnyAsync(ct => ct.Id == dto.ClientTypeId);
            if (!clientTypeExists) return ResponseHelper.ResponseError<ClientEditDto>(404, $"El nuevo tipo de cliente no existe.");

            clientEntity.ClientTypeId = dto.ClientTypeId;
            _context.Clients.Update(clientEntity);

            var userEntity = await _userManager.FindByIdAsync(clientEntity.UserId);
            userEntity.Name = dto.ClientName;
            await _userManager.UpdateAsync(userEntity);

            await _context.SaveChangesAsync();

            return ResponseHelper.ResponseSuccess<ClientEditDto>(200, $"{MessagesConstant.UPDATE_SUCCESS}");
        }

    }
}
