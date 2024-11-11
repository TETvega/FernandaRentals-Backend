using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FernandaRentals.Database.Entities
{
    [Table("event", Schema = "dbo")]
    public class EventEntity : BaseEntity
    {

        [Display(Name = "Id del Usuario")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        public Guid ClientId { get; set; }
        [ForeignKey(nameof(ClientId))]
        public virtual ClientEntity Client { get; set; }

        [Display(Name = "Fecha de Inicio")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Fecha de Fin")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Ubicación")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        public string Location { get; set; }

        [Display(Name = "SubTotal")] // SIN REQUIRED
        public decimal EventCost { get; set; }

        [Display(Name = "Descuento")] // SIN REQUIRED
        public decimal Discount { get; set; }

        [Display(Name = "Total a pagar")]
        public decimal Total { get; set; }


        // agrgando una lista de evendetaids 
        public virtual ICollection<DetailEntity> EventDetails { get; set; } = new List<DetailEntity>();
        
    }
}
