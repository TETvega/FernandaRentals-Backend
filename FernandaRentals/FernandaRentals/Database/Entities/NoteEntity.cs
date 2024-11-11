using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FernandaRentals.Database.Entities
{
    [Table("notes", Schema = "dbo")]
    public class NoteEntity : BaseEntity
    {
        [Display(Name = "Titulo de Nota")]
        [Column("title")]
        public string Title { get; set; }

        [Display(Name = "Evento Id")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [Column("event_id")]
        public Guid EventId { get; set; }
        [ForeignKey(nameof(EventId))]
        public virtual EventEntity Event { get; set; }

        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [StringLength(500, ErrorMessage = "La {0} no puede tener más de {1} caracteres.")]
        [Column("description")]
        public string Description { get; set; }

        // las comunes del campo de auditoria
        public virtual UserEntity CreatedByUser { get; set; }
        public virtual UserEntity UpdatedByUser { get; set; }
    }
}
