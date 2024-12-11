namespace FernandaRentals.Dtos.DashBoard.StadisticsDtos
{
    public class TComparation
    {
        public int NewTLast7Days { get; set; } 
        public int NewTPrevious7Days { get; set; } 
        public decimal PercentageChange { get; set; }

        public string Message { get; set; }
    }
}
