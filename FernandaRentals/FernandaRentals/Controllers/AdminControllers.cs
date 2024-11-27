using FernandaRentals.Dtos.Admin;
using FernandaRentals.Dtos.Auth;
using FernandaRentals.Dtos.Common;
using FernandaRentals.Dtos.DashBoard;
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

        [HttpGet("clients-data")]
        public async Task<ActionResult<ResponseDto<List<ClientsDataDto>>>> ClientsData()
        {
            var response = await _adminService.GetClientsData();
            return StatusCode(response.StatusCode, response);
        }
    }
}
