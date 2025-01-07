
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data; // Importa el namespace del contexto
using ProyectoCaritas.Models.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura EF Core con MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 40)) // Especifica la versión de MySQL
    ));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    SeedDatabase(dbContext);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


void SeedDatabase(ApplicationDbContext context)
{
    if (!context.Categories.Any())
    {
        context.Categories.AddRange(new[]
        {
            new Category { Id = 1, Name = "Alimentos" },
            new Category { Id = 2, Name = "Ropa" },
            new Category { Id = 3, Name = "Calzado" },
            new Category { Id = 4, Name = "Juguetes" },
            new Category { Id = 5, Name = "Material escolar" }
        });
    }

    if (!context.Centers.Any())
    {
        context.Centers.AddRange(new[]
        {
            new Center { Id = 1, Name = "Centro Caritas 1", Location = "Calle de la Caridad, 1", Manager = "Juan Pérez", CapacityLimit = 100, Phone = "123456789" },
            new Center { Id = 2, Name = "Centro Caritas 2", Location = "Calle de Dios, 2", Manager = "María López", CapacityLimit = 150, Phone = "987654321" }
        });
    }

    if (!context.DonationRequests.Any())
    {
        context.DonationRequests.AddRange(new[]
        {
            new DonationRequest { Id = 1, AssignedCenterId = 1, ShipmentDate = DateTime.Now, ReceptionDate = DateTime.Now, Status = "Recibido" },
            new DonationRequest { Id = 2, AssignedCenterId = 2, ShipmentDate = DateTime.Now, ReceptionDate = DateTime.Now, Status = "Recibido" }
        });
    }

    context.SaveChanges();
}

