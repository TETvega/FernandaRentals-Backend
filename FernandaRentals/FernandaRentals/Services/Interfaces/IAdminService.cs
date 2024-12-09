using FernandaRentals.Dtos.Admin;
using FernandaRentals.Dtos.Common;
using FernandaRentals.Dtos.DashBoard;

namespace FernandaRentals.Services.Interfaces
{
    public interface IAdminService
    {
        Task<ResponseDto<UserAdminCreateDto>> CreateAdminUser(UserAdminCreateDto dto);
        Task<ResponseDto<UserAdminEditDto>> EditAdminUser(UserAdminEditDto dto, string id);
        Task<ResponseDto<List<UserDto>>> GetAdminUsers();
        Task<ResponseDto<List<ClientsDataDto>>> GetClientsData();
        Task<ResponseDto<DashBoardDto>> GetDashBoardData();
        Task<ResponseDto<FinancialReportDto>> GetMonthlyFinancialReport(MonthDataDto monthData);
    }
}
