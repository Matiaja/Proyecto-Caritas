namespace ProyectoCaritas.Models.Entities
{
    public class DonationRequest
    {
        public int Id { get; set; }
        public  int? AssignedCenterId { get; set; } // Una Solicitud de Donación pertenece a cero o un Centro
        public  int? OrderLineId { get; set; } // Una Solicitud de Donación tiene cero o una linea de pedido
        public DateTime ShipmentDate { get; set; }
        public DateTime? ReceptionDate { get; set; }
        public required string Status { get; set; }

        public Center? AssignedCenter { get; set; } // Una Solicitud de Donación pertenece a cero o un Centro
        public OrderLine? OrderLine { get; set; } // Una Línea de Pedido tiene cero o una Solicitud.
    }
}
