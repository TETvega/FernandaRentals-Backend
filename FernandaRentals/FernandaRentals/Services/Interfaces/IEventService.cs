﻿using FernandaRentals.Dtos.CategoriesProduct;
using FernandaRentals.Dtos.Common;
using FernandaRentals.Dtos.Events;
using FernandaRentals.Dtos.Events.Helper_Dto;

namespace FernandaRentals.Services.Interfaces
{
    public interface IEventService
    {
        Task<ResponseDto<EventDto>> CancelEventAsync(Guid id);
        Task<ResponseDto<EventDto>> CreateEvent(EventCreateDto dto);
        Task<ResponseDto<EventDto>> EditEventAsync(EventEditDto dto, Guid id);
        Task<ResponseDto<EventDto>> GetEventById(Guid id);
        Task<ResponseDto<List<EventDto>>> GetAllEventsAsync(string opt);

        Task<ResponseDto<List<EventDto>>> GetAllEventsByUserIdAsync();
        Task<ResponseDto<List<ProductAvailabilityError>>> ValidateProductDatesWithAvailability(ProductAvailabilityDto dto);
    }
}
