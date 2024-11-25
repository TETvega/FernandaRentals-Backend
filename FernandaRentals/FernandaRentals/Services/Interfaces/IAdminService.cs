using FernandaRentals.Dtos.Common;
using FernandaRentals.Dtos.DashBoard;

namespace FernandaRentals.Services.Interfaces
{
    public interface IAdminService
    {
        Task<ResponseDto<DashBoardDto>> GetDashBoardData();
    }
}
