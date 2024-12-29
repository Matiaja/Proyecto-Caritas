namespace ProyectoCaritas.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public required int CategoryId { get; set; } // Un Producto pertenece solo a una Categoría. 
        public int? OrderLineId {  get; set; } // Un Producto tiene cero o una Linea de pedido.
        public required string Name { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public byte[]? Image { get; set; }

        public required Category Category { get; set; } // Un Producto pertenece solo a una Categoría. 
        public OrderLine? OrderLine { get; set; } // Un Producto tiene cero o una Linea de pedido.
        public ICollection<Stock>? Stocks { get; set; } // Un Producto tiene cero o más Stock.
    }
}
