namespace ProyectoCaritas.Models.Entities
{
    public class Center
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Location { get; set; }
        public required string Manager { get; set; }
        public int? CapacityLimit { get; set; }
        public required string Phone { get; set; }
        public string? Email { get; set; }

        public ICollection<Stock>? Stocks { get; set; } // Un Centro tiene cero o más Stocks.
        public ICollection<User>? Users { get; set; } // Un Centro tiene cero o más Usuarios.
        public ICollection<DonationRequest>? DonationRequests { get; set; }

    }
}
