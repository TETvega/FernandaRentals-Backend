﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FernandaRentals.Database.Entities
{ 
    [Table("products", Schema = "dbo")]
    public class ProductEntity : BaseEntity
    {
        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [Column("description")]
        [StringLength(501, ErrorMessage = "La {0} no puede tener más de {1} caracteres.")]
        public string Description { get; set; }


        [Display(Name = "Imagen Url del Producto")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [RegularExpression(@"^(https?://)?([\da-z.-]+)\.([a-z.]{2,6})([/\w .-]*)*/?$", ErrorMessage = "La URL de la imagen no es válida.")]
        [Column("url_image")]
        public string UrlImage { get; set; }

        [Display(Name = "Categoría Id")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [Column("category_id")]
        public Guid CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public virtual CategoryProductEntity Category { get; set; }

        [Display(Name = "Stock")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [Column("stock")]
        public int Stock { get; set; }

        [Display(Name = "Costo")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        [Column("price")]
        public decimal Price { get; set; } // se cambia de COST a PRICE para poder usar el paquete npm react-use-cart en frontend
        // las comunes del campo de auditoria
        public virtual UserEntity CreatedByUser { get; set; }
        public virtual UserEntity UpdatedByUser { get; set; }
    }
}
