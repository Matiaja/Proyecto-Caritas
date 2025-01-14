using ProyectoCaritas.Models.Entities;
namespace ProyectoCaritas.Models.DTOs
{
    public class GetCategoryDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<ProductDTO>? Products { get; set; }
    }
}
