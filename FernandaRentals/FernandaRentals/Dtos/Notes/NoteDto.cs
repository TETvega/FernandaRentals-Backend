using FernandaRentals.Database.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FernandaRentals.Dtos.Notes
{
    public class NoteDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid EventId { get; set; }
        public string Description { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
