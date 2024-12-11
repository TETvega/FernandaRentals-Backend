using FernandaRentals.Dtos.DashBoard.StadisticsDtos;

namespace FernandaRentals.Dtos.DashBoard
{
    public class DashboardComparationDto
    {
        public TComparation ClientComparation { get; set; }
        public TComparation ProductsComparation { get; set;}

        public TComparation EventsComparation { get; set; }

    }
}
