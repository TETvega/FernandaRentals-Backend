using FernandaRentals.Database.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FernandaRentals.Dtos.Events.Helper_Dto;
using FernandaRentals.Dtos.Client;
using FernandaRentals.Dtos.Notes;

namespace FernandaRentals.Dtos.Events
{
    public class EventDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public ClientDto Client { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public decimal EventCost { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string PaypalCaptureId { get; set; }

        // Incluimos el objeto completo del cliente

        public virtual ICollection<NoteDto> EventNotes { get; set; } = new List<NoteDto>();

        public virtual ICollection<DetailDto> EventDetails { get; set; } = new List<DetailDto>();
    }
}
