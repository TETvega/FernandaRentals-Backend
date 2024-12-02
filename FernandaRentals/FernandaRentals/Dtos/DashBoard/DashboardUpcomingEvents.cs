namespace FernandaRentals.Dtos.DashBoard
{
    public class DashboardUpcomingEvents
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
