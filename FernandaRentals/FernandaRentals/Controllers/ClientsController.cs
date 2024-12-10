using FernandaRentals.Dtos.Client;
using FernandaRentals.Dtos.Common;
using FernandaRentals.Dtos.DashBoard;
using FernandaRentals.Services;
using FernandaRentals.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FernandaRentals.Controllers
{
    [Route("api/clients")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }


        [HttpPut("{Id}")]
        public async Task<ActionResult<ResponseDto<ClientEditDto>>> UpdateClient(ClientEditDto dto, Guid id)
        {
            var response = await _clientService.EditClient(dto, id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
