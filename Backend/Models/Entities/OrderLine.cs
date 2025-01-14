namespace ProyectoCaritas.Models.Entities
{
    public class OrderLine
    {
        public int Id { get; set; }
        public int? RequestId { get; set; } // Una Línea de Pedido tiene cero o una Solicitud.
        public int? DonationRequestId { get; set; } // Una línea de pedido tiene cero o una Solicitud de donación.
        public required int Quantity { get; set; }
        public string? Description { get; set; }
        public int? ProductId { get; set; } // Una Línea de pedido está compuesta por cero o un Producto.

        public Request? Request { get; set; } // Una Línea de Pedido tiene cero o una Solicitud.
        public DonationRequest? DonationRequest { get; set; } // Una línea de pedido tiene cero o una Solicitud de donación.
        public Product? Product { get; set; } // Una Línea de pedido está compuesta por cero o un Producto.
    }
}
