namespace ProyectoCaritas.Models.DTOs
{
    public class AddProductDTO
    {
        public required int CategoryId { get; set; }  
        public required string Name { get; set; }
        public string? Code { get; set; }
    }
}