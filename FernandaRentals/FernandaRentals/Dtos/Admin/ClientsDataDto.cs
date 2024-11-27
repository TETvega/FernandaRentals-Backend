namespace FernandaRentals.Dtos.Admin
{
    public class ClientsDataDto
    {
        public Guid ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public string ClientTypeName { get; set; }
        public Guid ClientTypeId { get; set; }
        public int TotalPastEvents { get; set; }
        public int TotalUpcomingEvents { get; set; }
    }
}
