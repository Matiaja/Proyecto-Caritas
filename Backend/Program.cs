
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
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    await RoleInitializer.CreateRoles(services);
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