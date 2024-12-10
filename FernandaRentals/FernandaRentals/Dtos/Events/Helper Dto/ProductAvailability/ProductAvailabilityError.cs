using FernandaRentals.Database.Entities;

namespace FernandaRentals.Dtos.Events.Helper_Dto
{
    public class ProductAvailabilityError
    {
        public ProductEntity Product { get; set; }
        public DateTime UnavailableDate { get; set; }
    }
}
