using FernandaRentals.Database.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FernandaRentals.Dtos.Events.Helper_Dto;
using FernandaRentals.Dtos.Notes;

namespace FernandaRentals.Dtos.Events
{
    public class EventCreateDto
    {
        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El {0} del evento es requerido.")]
        public string Name { get; set; }

        [Display(Name = "Fecha de Inicio")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Fecha de Fin")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Ubicación")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        public string Location { get; set; }

        [Display(Name = "Id transaccion de Paypal")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        public string PaypalCaptureId { get; set; }
        public IEnumerable<EventProducDto> Productos { get; set; }

        public IEnumerable<NoteDto> EventNotes { get; set; } 
    }
}
