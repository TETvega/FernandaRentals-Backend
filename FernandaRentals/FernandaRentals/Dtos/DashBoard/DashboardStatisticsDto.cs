using FernandaRentals.Dtos.DashBoard.StadisticsDtos;

namespace FernandaRentals.Dtos.DashBoard
{
    public class DashboardStatisticsDto
    {
        public decimal GrossProfit { get; set; }  // Utilidad Bruta
        public List<MonthlyProfitDto> GrossProfitByMonth { get; set; } // Utilidad Bruta por Mes
        public List<MonthlyProfitDto> NetProfitByMonth { get; set; } // Utilidad Neta por Mes
        public List<ProductRevenueDto> ProductsRevenueDistribution { get; set; } // Grafico en Pastel de productos 
    }
}
