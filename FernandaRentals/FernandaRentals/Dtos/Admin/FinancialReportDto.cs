namespace FernandaRentals.Dtos.Admin
{
    public class FinancialReportDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalDiscounts { get; set; }
        public int EventCount { get; set; }
        public decimal AverageRevenue { get; set; }
    }
}
