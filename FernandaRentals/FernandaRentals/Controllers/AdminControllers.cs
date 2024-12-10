using FernandaRentals.Dtos.Admin;
using FernandaRentals.Dtos.Auth;
using FernandaRentals.Dtos.Common;
using FernandaRentals.Dtos.DashBoard;
using FernandaRentals.Dtos.Products;
using FernandaRentals.Services;
using FernandaRentals.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FernandaRentals.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminControllers : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminControllers(IAdminService adminService)
        {
           _adminService = adminService;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<ResponseDto<DashBoardDto>>> DashBoardData()
        {
            var response = await _adminService.GetDashBoardData();
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("financials/monthly")]
        public async Task<ActionResult<ResponseDto<FinancialReportDto>>> GetMonthlyFinancialReport(MonthDataDto dto)
        {
            var response = await _adminService.GetMonthlyFinancialReport(dto);
            return StatusCode(response.StatusCode, response);
        }



        [HttpGet("clients-data")]
        public async Task<ActionResult<ResponseDto<List<ClientsDataDto>>>> ClientsData()
        {
            var response = await _adminService.GetClientsData();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("admin-users")]
        public async Task<ActionResult<ResponseDto<List<UserDto>>>> GetAdminUsers()
        {
            var response = await _adminService.GetAdminUsers();
            return StatusCode(response.StatusCode, response);
        }


        [HttpPost("create-admin")]
        public async Task<ActionResult<ResponseDto<UserAdminCreateDto>>> CreateUsrAdmin(UserAdminCreateDto dto)
        {
            var response = await _adminService.CreateAdminUser(dto);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("edit-admin/{Id}")]
        public async Task<ActionResult<ResponseDto<UserAdminEditDto>>> Edit(UserAdminEditDto dto, string id)
        {
            var response = await _adminService.EditAdminUser(dto, id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
