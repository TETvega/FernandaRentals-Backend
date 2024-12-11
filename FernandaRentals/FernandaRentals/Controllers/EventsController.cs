using FernandaRentals.Constants;
using FernandaRentals.Dtos.Common;
using FernandaRentals.Dtos.Events;
using FernandaRentals.Dtos.Events.Helper_Dto;
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

        [HttpGet("get/{opt}")]
        [Authorize(Roles = $"{RolesConstants.ADMIN}")]
        public async Task<ActionResult<ResponseDto<EventDto>>> GetAll(string opt)
        {
            var response = await _eventService.GetAllEventsAsync(opt);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{Id}")]
        [Authorize]
        public async Task<ActionResult<ResponseDto<EventDto>>> GetEventById(Guid id)
        {
            var response = await _eventService.GetEventById(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("my-events")]
        [Authorize]
        public async Task<ActionResult<ResponseDto<EventDto>>> GetEventsByUserId()
        {
            var response = await _eventService.GetAllEventsByUserIdAsync();
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        [Authorize(Roles = $"{RolesConstants.CLIENT}")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<EventDto>>> CreateEvent(EventCreateDto dto)
        {
            var response = await _eventService.CreateEvent(dto);
            return StatusCode(response.StatusCode, response);
        }


        [HttpPut("{id}")]
        [Authorize(Roles = $"{RolesConstants.ADMIN}, {RolesConstants.CLIENT}")]
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
        [Authorize(Roles = $"{RolesConstants.ADMIN}, {RolesConstants.CLIENT}")]
        public async Task<ActionResult<ResponseDto<EventDto>>> CancelEvent(Guid id)
        {
            var response = await _eventService.CancelEventAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("validate-products")]
        //[Authorize(Roles = $"{RolesConstants.ADMIN}, {RolesConstants.CLIENT}")]
        public async Task<ActionResult<ResponseDto<List<ProductAvailabilityError>>>> ValidateProducts (ProductAvailabilityDto dto)
        {
            var response = await _eventService.ValidateProductDatesWithAvailability(dto);
            return StatusCode(response.StatusCode, response);
        }
    }
}
