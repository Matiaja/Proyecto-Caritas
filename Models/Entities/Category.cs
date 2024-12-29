namespace ProyectoCaritas.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public ICollection<Product>? Products { get; set; } // Una Categoría tiene cero o más Productos.
    }
}
