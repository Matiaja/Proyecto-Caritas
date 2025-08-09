namespace ProyectoCaritas.Models.DTOs
{
    public class AddDonationRequestDTO
    {
        public int AssignedCenterId { get; set; }
        public int OrderLineId { get; set; }
        public int Quantity { get; set; }
    }
}
