
using FernandaRentals.Dtos.CategoriesProduct.HelperDto;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FernandaRentals.Database.Entities
{
    [Table("categories_product", Schema = "dbo")]
    public class CategoryProductEntity : BaseEntity
    {
        [Display(Name = "Descripción")]
        [StringLength(100, ErrorMessage = "La {0} debe tener un maximo de {1} caracteres de longitud.")]
        [Column("description")]
        public string Description { get; set; }

        public virtual IEnumerable<ProductDtoForCategoryProduct> ProductsOfCategory { get; set; }

        // las comunes del campo de auditoria
        public virtual UserEntity CreatedByUser { get; set; }
        public virtual UserEntity UpdatedByUser { get; set; }
    }
}