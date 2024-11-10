using System.ComponentModel.DataAnnotations;

namespace FernandaRentals.Dtos.Events.Helper_Dto
{
    public class EventProducDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }
    }
}

