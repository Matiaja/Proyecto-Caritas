namespace ProyectoCaritas.Models.Entities
{
    public class Stock
    {
        public int Id { get; set; }
        public required int CenterId { get; set; } // Un Stock pertenece a solo a un Centro.
        public int? ProductId { get; set; } // Un Stock tiene cero o un Producto.
        public DateTime EntryDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public double Weight { get; set; }
        public required string Status { get; set; }

        public required Center Center { get; set; } // Un Stock pertenece a solo a un Centro.
        public Product? Product { get; set; } // Un Stock tiene cero o un Producto.
    }
}
