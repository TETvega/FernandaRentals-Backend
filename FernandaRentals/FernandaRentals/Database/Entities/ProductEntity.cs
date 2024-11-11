using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FernandaRentals.Database.Entities
{ 
    [Table("product", Schema = "dbo")]
    public class ProductEntity : BaseEntity
    {
        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [StringLength(501, ErrorMessage = "La {0} no puede tener más de {1} caracteres.")]
        public string Description { get; set; }

        [Display(Name = "Imagen Url del Producto")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [RegularExpression(@"^(https?://)?([\da-z.-]+)\.([a-z.]{2,6})([/\w .-]*)*/?$", ErrorMessage = "La URL de la imagen no es válida.")]
        public string UrlImage { get; set; }

        [Display(Name = "Categoría Id")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        public Guid CategoryId { get; set; }
        public virtual CategoryProductEntity Category { get; set; }

        [Display(Name = "Stock")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        public int Stock { get; set; }

        [Display(Name = "Costo")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        public decimal Cost { get; set; }

    }
}
