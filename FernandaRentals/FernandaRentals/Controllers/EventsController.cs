using FernandaRentals.Constants;
using FernandaRentals.Dtos.common;
using FernandaRentals.Dtos.Events;
using FernandaRentals.Dtos.Products;
using FernandaRentals.Services;
using FernandaRentals.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FernandaRentals.Controllers
{
    [ApiController]
    [Route("api/eventos")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        public EventsController(IEventService eventService) 
        {
            _eventService = eventService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ResponseDto<EventDto>>> GetAll()
        {
            var response = await _eventService.GetAllEventsAsync();
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{Id}")]
        [Authorize]
        public async Task<ActionResult<ResponseDto<EventDto>>> GetEventById(Guid id)
        {
            var response = await _eventService.GetEventById(id);
            return StatusCode(response.StatusCode, response);
        }

      [HttpPost]
        //[Authorize(Roles = $"{RolesConstant.CLIENT}")]
        //[Authorize(Roles = $"{RolesConstant.ADMIN}")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<EventDto>>> CreateEvent(EventCreateDto dto)
        {
            var response = await _eventService.CreateEvent(dto);
            return StatusCode(response.StatusCode, response);
        }


        [HttpPut("{id}")]
        [Authorize(Roles = $"{RolesConstant.ADMIN}, {RolesConstant.CLIENT}")]
        public async Task<ActionResult<ResponseDto<EventDto>>> Edit(EventEditDto dto, Guid id)
        {
            var response = await _eventService.EditEventAsync(dto, id);

            return StatusCode(response.StatusCode, new
            {
                response.Status,
                response.Message,

            });
        }

        [HttpDelete("{Id}")]
        [Authorize(Roles = $"{RolesConstant.ADMIN}, {RolesConstant.CLIENT}")]
        public async Task<ActionResult<ResponseDto<EventDto>>> CancelEvent(Guid id)
        {
            var response = await _eventService.CancelEventAsync(id);
            return StatusCode(response.StatusCode, response);
        }
    }
}
