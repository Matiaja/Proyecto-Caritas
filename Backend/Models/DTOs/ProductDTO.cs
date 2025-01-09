using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Models.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public required int CategoryId { get; set; }  
        public required string Name { get; set; }

        public required Category Category { get; set; }
    }
}