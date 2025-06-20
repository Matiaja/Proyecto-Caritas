namespace ProyectoCaritas.Models.Entities
{
    public class OrderLine
    {
        public int Id { get; set; }
        public int? RequestId { get; set; } // Una Línea de Pedido tiene cero o una Solicitud.
        public required string Status { get; set; }
        public required int Quantity { get; set; }
        public string? Description { get; set; }
        public int? ProductId { get; set; } // Una Línea de pedido está compuesta por cero o un Producto.

        public Request? Request { get; set; } // Una Línea de Pedido tiene cero o una Solicitud.
        public ICollection<DonationRequest>? DonationRequests { get; set; } // Una línea de pedido tiene cero o muchas donaciones.
        public Product? Product { get; set; } // Una Línea de pedido está compuesta por cero o un Producto.
    }
}
