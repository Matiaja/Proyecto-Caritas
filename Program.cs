
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data; // Importa el namespace del contexto
using Microsoft.AspNetCore.Identity;
using ProyectoCaritas.Models.Entities;
using ProyectoCaritas.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configurar Swagger (OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura EF Core con MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")) // Especifica la versión de MySQL
    ));

// Configurar Identity
builder.Services.AddIdentity<User, IdentityRole>(
    options =>
    {
        //Password
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;

        //Require Email 
        options.User.RequireUniqueEmail = true; // Requiere un email único
        options. SignIn.RequireConfirmedEmail = false;

        //Lockout
        options.Lockout.AllowedForNewUsers = false;
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Clave secreta para firmar el token (usa una clave segura)
var key = Encoding.ASCII.GetBytes("SuperSecureKey1234!·$%&/()=asdfasdf");

// Configuración de la autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false, // Puedes ajustar según tu necesidad
        ValidateAudience = false, // Puedes ajustar según tu necesidad
        RequireExpirationTime = true,
        ValidateLifetime = true
    };
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
<<<<<<< HEAD
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    SeedDatabase(dbContext);
=======
    await RoleInitializer.CreateRoles(services);
>>>>>>> alvaro
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Habilitar redirección HTTP
app.UseHttpsRedirection();

// Agrega autenticación antes de la autorización.
app.UseAuthentication(); 
app.UseAuthorization();

// Mapear controladores
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

