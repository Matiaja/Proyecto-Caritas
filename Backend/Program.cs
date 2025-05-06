
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data; // Importa el namespace del contexto
using Microsoft.AspNetCore.Identity;
using ProyectoCaritas.Models.Entities;
using ProyectoCaritas.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Configurar Swagger (OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    c =>
    {
        c.SwaggerDoc("v1", new() { Title = "Tu API", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Introduce el token JWT como: Bearer {token}"
        });

        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    }

    );

//// Configura EF Core con MySQL
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseMySql(
//        builder.Configuration.GetConnectionString("DefaultConnection"),
//        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")) // Especifica la versión de MySQL
//    ));
//builder.Services.AddScoped<OrderLineService>();

const string aivenCaCert = @"-----BEGIN CERTIFICATE-----
MIIETTCCArWgAwIBAgIULo9iyIDZDt32tE69r8EKSNjPlGQwDQYJKoZIhvcNAQEM
BQAwQDE+MDwGA1UEAww1NzlmNmNmZDQtNjE0Ny00MDA4LWI5MmYtYWZmYzk4MzU2
ZWUxIEdFTiAxIFByb2plY3QgQ0EwHhcNMjUwNTAzMTg0NDU1WhcNMzUwNTAxMTg0
NDU1WjBAMT4wPAYDVQQDDDU3OWY2Y2ZkNC02MTQ3LTQwMDgtYjkyZi1hZmZjOTgz
NTZlZTEgR0VOIDEgUHJvamVjdCBDQTCCAaIwDQYJKoZIhvcNAQEBBQADggGPADCC
AYoCggGBAOS1rMR8B8mj4GMRZvbkfc6UrXxLlqvNh6A8CYTt+G65B0GM/ccbreDv
mHn4FrHS/jM9iA/BnQH5kduq/XDOqnzkeexn0hwIaD3qjgx+ag9fXC2hak1Qao4E
19fQ3NkTUgaJx3/oco1/dEPYa1MwOXr+7ZitAnXHtbMIOB9VhBas2XhL6DEmevED
eUAXoa5vRr5NJ78kVnKpLKQEJIHzlbGBRgKn/qGHEOjbBft9vTuHeMNoqAggJvnz
F+72Z73hyPhvqdMKYdzkpN061RTYfG3XOM1of9aUYkYQHvLVtWOHRVM5QiiKFtnE
bOfEq5vYc4SebU3jmwYZnqgEPsqPAbaA9g3PP6o1wCgkIr8GxiMJ894YM1aqAhRg
gnG+QknP+9LuNCJXAcJPhMM5Ld4WJzS5JrqfJegCde0POEarXApKPaeG316vNAGJ
MSyjLmr5zMuq8bRrBDTPwo5rxaE88521jlCscDQonE41NlZRrwRE5oMWXQbd7eeZ
fhf4G42OPwIDAQABoz8wPTAdBgNVHQ4EFgQU9Q9PChAIAG0Dxl08P8+PG1xyIHow
DwYDVR0TBAgwBgEB/wIBADALBgNVHQ8EBAMCAQYwDQYJKoZIhvcNAQEMBQADggGB
AGjsTYEHHpd/4f5EQcdD/9TpkN6s5nL2r5Si+nUx7BXn3XfnCG0kW4hPGZGLwoX0
p1DtJJVDeXQ9iKQIk5hz+BCmT8JhREJtACDNHdLxIzsEWsFX20Ik1Oy635PCvcDb
PZ4B+XfdDglapDRFNVF4mnRY2p+H1aymIk09jaEAaFzuMguM9Kiemvgn4xOVlM3G
5jaUTSZnGHmjcC2sJYvkwbaZ7bEi6qWTkXzNQaaBU0wJ71JJJh/80LC75qoSFZfr
AWx2OEzwgm/WFLzxbNMJ5iCSO35XtQqY5OHKMZk1DVuFaEU/w+MImWfeyf8rS2L6
MrujuEq1XZATMfUFe3GGV05ksq7DG6X8i5TkvVgCic7/Y7jZk0/e2dcYk72mU/SM
cAUXI57/HpdZSd4u/YsSxZaehtXrp/VA5wp2zVSNpC+02mrsD2kD/JsItVMIcUtA
PQGjxEsh0dTACn3p3WfaaGMW/IVd90eNkQB85JYH6eLyLZYr7qSSvLGDF6nDXhlC
fg==
-----END CERTIFICATE-----";

// Crea un archivo temporal
var tempCertPath = Path.Combine(Path.GetTempPath(), "aiven-ca.pem");
File.WriteAllText(tempCertPath, aivenCaCert);

var connectionString = builder.Configuration.GetConnectionString("AivenMySQLConnection")
                        .Replace("ca.pem", tempCertPath);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }
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
        options.SignIn.RequireConfirmedEmail = false;

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
        ValidateLifetime = true,
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await RoleInitializer.CreateRoles(services);
        Console.WriteLine("Roles inicializados correctamente.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error durante la inicialización de roles.");
    }
}

// Capturar errores globales
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Se produjo un error en la solicitud.");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { message = "Error interno del servidor" });
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Habilitar CORS
app.UseCors("AllowAngular");

// Habilitar redirección HTTP
app.UseHttpsRedirection();

// Agrega autenticación antes de la autorización.
app.UseAuthentication();
app.UseAuthorization();

// Mapear controladores
app.MapControllers();

app.Run();