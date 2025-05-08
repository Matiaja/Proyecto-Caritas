using System.ComponentModel.DataAnnotations;

namespace ProyectoCaritas.Models.DTOs
{
    public class OrderLineDTO
    {
        public int Id { get; set; }
        public int? RequestId { get; set; }
        public RequestBasicDTO? Request { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int Quantity { get; set; }
        public string? Description { get; set; }
        public int? ProductId { get; set; }
        public ProductDTO? Product { get; set; }
        public ICollection<GetDonationRequestDTO>? DonationRequests { get; set; } = new List<GetDonationRequestDTO>();
    }
}