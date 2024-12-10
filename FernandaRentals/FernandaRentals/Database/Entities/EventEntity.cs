using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FernandaRentals.Dtos.Notes;

namespace FernandaRentals.Database.Entities
{
    [Table("events", Schema = "dbo")]
    public class EventEntity : BaseEntity
    {

        [Display(Name = "Id del Usuario")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [Column("user_id")]
        public Guid ClientId { get; set; }
        [ForeignKey(nameof(ClientId))]
        public virtual ClientEntity Client { get; set; }

        [Display(Name = "Fecha de Inicio")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Fecha de Fin")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Ubicación")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("location")]
        public string Location { get; set; }
        /// <summary>
        /// TOTAL DE LOS PAGOS Y SUMATORIAS , DESCUENTOS, RETENES
        /// </summary>
        [Display(Name = "SubTotal")] // SIN REQUIRED
        [Column("subtotal")]

        public decimal EventCost { get; set; }

        [Display(Name = "Descuento")] // SIN REQUIRED
        [Column("discount")]
        public decimal Discount { get; set; }

        [Display(Name = "Total a pagar")]
        [Column("total")]
        public decimal Total { get; set; }

        [Display(Name = "Id transaccion de Paypal")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [Column("paypal_capture_id")]
        [StringLength(50)]
        public string PaypalCaptureId { get; set; }


        // agrgando una lista de evendetaids  y listaNotas
        public virtual ICollection<DetailEntity> EventDetails { get; set; } = new List<DetailEntity>();

        public virtual ICollection<NoteEntity> EventNotes { get; set; } = new List<NoteEntity>();
        // las comunes del campo de auditoria
        public virtual UserEntity CreatedByUser { get; set; }
        public virtual UserEntity UpdatedByUser { get; set; }

    }
}
