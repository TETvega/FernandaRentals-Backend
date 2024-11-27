using FernandaRentals.Dtos.Admin;
using FernandaRentals.Dtos.Common;
using FernandaRentals.Dtos.DashBoard;

namespace FernandaRentals.Services.Interfaces
{
    public interface IAdminService
    {
        Task<ResponseDto<List<ClientsDataDto>>> GetClientsData();
        Task<ResponseDto<DashBoardDto>> GetDashBoardData();
    }
}
