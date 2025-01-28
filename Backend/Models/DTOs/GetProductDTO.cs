namespace ProyectoCaritas.Models.DTOs
{
    public class GetProductDTO
    {
        public int Id { get; set; }
        public required int CategoryId { get; set; }
        public required string Name { get; set; }
        public string? Code { get; set; }
    }
}
