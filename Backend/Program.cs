﻿
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data; // Importa el namespace del contexto
using Microsoft.AspNetCore.Identity;
using ProyectoCaritas.Models.Entities;
using ProyectoCaritas.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.Text.Json.Serialization;
using ProyectoCaritas.Hubs;
using ProyectoCaritas.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new DateTimeUtcConverter());

    });
builder.Services.AddSignalR();

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

// Configura EF Core con MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")) // Especifica la versión de MySQL
    ));
builder.Services.AddScoped<OrderLineService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IRequestStatusService, RequestStatusService>();
builder.Services.AddScoped<DonationRequestService>();
builder.Services.AddHttpClient();
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
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]!);

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

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Query["access_token"];

            // Si es una solicitud a SignalR
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(token) &&
                path.StartsWithSegments("/notificationHub"))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        },
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

// Mapear el hub
app.MapHub<NotificationHub>("/notificationHub");

app.Run();