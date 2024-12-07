using System.ComponentModel.DataAnnotations;

namespace FernandaRentals.Dtos.Events.Helper_Dto
{
    public class ProductAvailabilityDto
    {
        [Display(Name = "Fecha de Inicio")]
        [Required(ErrorMessage = "La {0} del evento es requerida.")]
        public DateTime EventStartDate { get; set; }

        [Display(Name = "Fecha de Fin")]
        [Required(ErrorMessage = "La {0} del evento es requerida.")]
        public DateTime EventEndDate { get; set; }

        [Display(Name = "Productos")]
        [Required(ErrorMessage = "Los {0} a rentar son requeridos si quieres crear un evento.")]
        public List<EventProducDto> Products { get; set; }
    }
}
