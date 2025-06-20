using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Models.DTOs
{
    public class GetCenterDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Location { get; set; }
        public required string Manager { get; set; }
        public int? CapacityLimit { get; set; }
        public required string Phone { get; set; }
        public string? Email { get; set; }

        public ICollection<GetStockDTO>? Stocks { get; set; }
        public ICollection<UserDTO>? Users { get; set; }
        public ICollection<GetDonationRequestDTO>? DonationRequests { get; set; }

    }
}
