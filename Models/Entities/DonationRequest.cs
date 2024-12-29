namespace ProyectoCaritas.Models.Entities
{
    public class DonationRequest
    {
        public int Id { get; set; }
        public  int? AssignedCenterId { get; set; } // Una Solicitud de Donación pertenece a cero o un Centro
        public DateTime ShipmentDate { get; set; }
        public DateTime? ReceptionDate { get; set; }
        public required string Status { get; set; }

        public Center? AssignedCenter { get; set; } // Una Solicitud de Donación pertenece a cero o un Centro
        public ICollection<OrderLine>? OrderLines { get; set; } // Una Línea de Pedido tiene cero o una Solicitud.
    }
}
