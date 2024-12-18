﻿using FernandaRentals.Dtos.ClientType;
using FernandaRentals.Dtos.Common;

namespace FernandaRentals.Services.Interfaces
{
    public interface IClientTypeService
    {
        Task<ResponseDto<List<ClientTypeDto>>> GetClientsTypesListAsync();
        Task<ResponseDto<ClientTypeDto>> GetClientTypeAsync(Guid id);
        Task<ResponseDto<ClientTypeDto>> CreateClientTypeAsync(ClientTypeCreateDto dto);
        Task<ResponseDto<ClientTypeDto>> EditClientTypeAsync(ClientTypeEditDto dto, Guid id);
        Task<ResponseDto<ClientTypeDto>> DeleteClientTypeAsync(Guid id);
    }
}
