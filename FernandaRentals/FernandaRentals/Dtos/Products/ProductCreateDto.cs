﻿using FernandaRentals.Database.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FernandaRentals.Dtos.Products
{
    public class ProductCreateDto
    {
        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "El {0} de la categoria es requerido.")]
        public string Name { get; set; }


        [Display(Name = "Descripción")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [StringLength(500, ErrorMessage = "La {0} no puede tener más de {1} caracteres.")]
        public string Description { get; set; }

        [Display(Name = "Imagen Url del Producto")]
        [Required(ErrorMessage = "La {0} es obligatoria.")]
        [RegularExpression(@"^(https?://)?([\da-z.-]+)\.([a-z.]{2,6})([/\w .-]*)*/?$", ErrorMessage = "La URL de la imagen no es válida.")]
        [Column("url_image")]
        public string UrlImage { get; set; }


        [Display(Name = "Categoría Id")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        public Guid CategoryId { get; set; }


        [Display(Name = "Stock")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        public int Stock { get; set; }


        [Display(Name = "precio")]
        [Required(ErrorMessage = "El {0} es obligatorio.")]
        public decimal Price { get; set; }
    }
}
