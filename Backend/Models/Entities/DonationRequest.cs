namespace ProyectoCaritas.Models.Entities
{
    public class DonationRequest
    {
        public int Id { get; set; }
        public int AssignedCenterId { get; set; } // Una Solicitud de Donación pertenece a cero o un Centro
        public int OrderLineId { get; set; } // Una Solicitud de Donación tiene cero o una linea de pedido
        public int Quantity { get; set; } // Cantidad de productos donados
        public DateTime AssignmentDate { get; set; } // Fecha de asignación de la solicitud
        public required string Status { get; set; }
        public DateTime? LastStatusChangeDate { get; set; } // Fecha del último cambio de estado

        public Center? AssignedCenter { get; set; } // Una Solicitud de Donación pertenece a cero o un Centro
        public OrderLine? OrderLine { get; set; } // Una Línea de Pedido tiene cero o una Solicitud.
        public ICollection<DonationRequestStatusHistory>? StatusHistory { get; set; }
    }
}
