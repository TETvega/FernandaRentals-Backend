using FernandaRentals.Dtos.DashBoard.StadisticsDtos;

namespace FernandaRentals.Dtos.DashBoard
{
    public class DashboardTopsDto
    {
        public List<TopProductDto> TopRequestedProducts { get; set; }
        public List<TopProductDto> LeastRequestedProducts { get; set; }
        public List<TopProductDto> TopRevenueProducts { get; set; }
    }
}
