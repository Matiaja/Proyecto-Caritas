using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoCaritas.Data;

namespace ProyectoCaritas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseTestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DatabaseTestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // 1. Prueba de conexión básica
                var canConnect = await _context.Database.CanConnectAsync();

                if (!canConnect)
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "No se pudo conectar a la base de datos"
                    });
                }

                // 2. Consulta para obtener la versión (forma corregida)
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT VERSION()";
                    var dbVersion = (await command.ExecuteScalarAsync())?.ToString();

                    // 3. Consulta adicional para verificar tablas
                    var tables = new List<string>();
                    try
                    {
                        command.CommandText = "SHOW TABLES";
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                tables.Add(reader.GetString(0));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al obtener tablas: {ex.Message}");
                    }

                    return Ok(new
                    {
                        success = true,
                        message = "Conexión exitosa a MySQL en Aiven",
                        version = dbVersion,
                        tables = tables
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al conectar con la base de datos",
                    error = ex.Message,
                    details = ex.InnerException?.Message,
                });
            }
        }
    }
}
