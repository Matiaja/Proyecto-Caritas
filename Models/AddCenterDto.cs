namespace ProyectoCaritas.Models
{
    public class AddCenterDto
    {
        public required string Name { get; set; }
        public required string Location { get; set; }
        public required string Manager { get; set; }
        public int CapacityLimit { get; set; }
        public required string Phone { get; set; }
        public string? Email { get; set; }

    }
}
