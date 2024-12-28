namespace ProyectoCaritas.Models.Entities
{
    public class Center
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Manager { get; set; }
        public int CapacityLimit { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public ICollection<Stock> Stocks { get; set; }
    }
}
