using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Models.Entities;

namespace ProyectoCaritas.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Center> Centers { get; set; }
        public DbSet<DonationRequest> DonationRequests { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relación uno a uno entre DonationRequest y OrderLine
            modelBuilder.Entity<DonationRequest>()
                .HasOne(d => d.OrderLine)  // DonationRequest tiene una OrderLine
                .WithOne(o => o.DonationRequest)  // OrderLine tiene una DonationRequest
                .HasForeignKey<DonationRequest>(d => d.OrderLineId)  // Clave foránea en DonationRequest
                .OnDelete(DeleteBehavior.SetNull); // Define el comportamiento de eliminación, si es necesario

            modelBuilder.Entity<OrderLine>()
                .HasOne(o => o.DonationRequest)
                .WithOne(d => d.OrderLine)
                .HasForeignKey<OrderLine>(o => o.DonationRequestId)
                .OnDelete(DeleteBehavior.SetNull); // Establecer el comportamiento de eliminación
                
            base.OnModelCreating(modelBuilder);
        }

    }

}
