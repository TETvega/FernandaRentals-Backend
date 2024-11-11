using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FernandaRentals.Database.Entities
{
    [Table("clients", Schema = "dbo")]
    public class ClientEntity: BaseEntity
    {

        [Display(Name = "Usuario Id")]
        [Required(ErrorMessage = "El {0} es requerido.")]

        public string UserId { get; set; }
        public virtual UserEntity User { get; set; }


        [Display(Name = "Tipo de Cliente Id")]
        [Required(ErrorMessage = "El {0} es requerido.")]
        
        public Guid ClientTypeId { get; set; }
        public virtual ClientTypeEntity ClientType { get; set; }
    }
}
