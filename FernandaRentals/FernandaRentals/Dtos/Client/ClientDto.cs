namespace FernandaRentals.Dtos.Client
{
    public class ClientDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string ClientType { get; set; } 
        public decimal ClientTypeDiscount { get; set; }
    }
}
