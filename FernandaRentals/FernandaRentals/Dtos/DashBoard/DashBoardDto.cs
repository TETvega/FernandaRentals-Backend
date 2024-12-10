using FernandaRentals.Database.Entities;

namespace FernandaRentals.Dtos.DashBoard
{
    public class DashBoardDto
    {
        public int TotalProducts { get; set; }
        public int TotalUpcomingEvents { get; set; }
        public int TotalClients { get; set; }
        public List<DashboardUpcomingEvents> UpcomingEvents { get; set; }
    }
}
