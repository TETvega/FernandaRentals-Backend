using FernandaRentals.Dtos.Client;
using FernandaRentals.Dtos.Common;

namespace FernandaRentals.Services.Interfaces
{
    public interface IClientService
    {
        Task<ResponseDto<ClientEditDto>> EditClient(ClientEditDto dto, Guid id);
    }
}
