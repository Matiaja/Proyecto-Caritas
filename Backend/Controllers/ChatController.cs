using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;


namespace ProyectoCaritas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ChatController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] List<MessageDto> messages)
        {
            try
            {
                var apiKey = _configuration["Groq:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                    return StatusCode(500, "API Key no configurada");

                // Convertimos los mensajes al formato esperado por Groq
                var groqMessages = messages.Select(m => new
                {
                    role = m.Role?.ToLower() ?? "user",
                    content = m.Content ?? string.Empty
                });

                var body = new
                {
                    model = "meta-llama/llama-4-scout-17b-16e-instruct", // Modelo específico proporcionado por Groq
                    messages = new List<object>
            {
            new {
                    role = "system",
                    content = @"
                    Sos el asistente del sistema de gestión de Cáritas Argentina. Guiá al usuario con pasos simples, en español de Argentina, usando nombres legibles (producto, centro, estado) y no IDs.

                    Capacidades (breve):
                    - Donaciones: alta con producto, cantidad, estado, fecha, categoría y origen/observaciones.
                    - Stock: entradas/salidas con historial (tipo, cantidad, fecha, descripción, origen, vencimiento, peso/lote).
                    - Visibilidad: depósito central ve todos los centros; cada parroquia solo su propio stock.
                    - Solicitudes: crear/editar, líneas con producto/cantidad/descr., urgencia y estado; asignar productos; seguimiento (Pendiente/Asignada/En tránsito/Completada/Cancelada).
                    - Distribución: preparar envíos y registrar movimientos asociados.
                    - Reportes: inventario, movimientos, solicitudes, vencimientos.
                    - PDFs: Detalle de Solicitud y Detalle de Stock (con nombres y tabla de movimientos filtrados).
                    - Acceso/roles: respetar permisos por rol y centro.
                    - Asignación: asignar productos a solicitudes, con validación de stock.
                    - Notificaciones: recibir alertas de asignaciones y rechazos.
                    - Chat: responder preguntas sobre procesos, productos, stock y solicitudes.
                    - PNUD: registrar compras y bolsones con tipo pnud que se obtienen mediante donaciones del estado, y deben tener una trazabilidad especial debido a ello, ademas se debe imprimir un certificado al entregar el bolsón.

                    Estilo y extensión:
                    - Sé claro y natural. Máximo 5–8 líneas o 3–6 pasos.
                    - Empezá directo al grano; evitá tecnicismos.
                    - Mostrá la ruta: ""Menú > Solicitudes > Detalle > Generar PDF"" cuando aplique.

                    Contexto y memoria:
                    - Usá el historial del chat para no repetir lo ya dicho.
                    - No vuelvas a saludar si ya saludaste.
                    - No repitas ""iniciar sesión"" salvo que el usuario tenga un problema de acceso.
                    - Recordá datos mencionados (centro, rol, filtros de fecha/tipo, solicitud o producto en foco) y continuá desde ahí.

                    Buenas prácticas:
                    - Antes de guardar o generar PDFs: confirmá cantidades y fechas de vencimiento; sugerí usar filtros.
                    - Si faltan datos clave, primero hacé 1–2 preguntas cortas (p. ej., centro, fecha, producto).

                    Permisos:
                    - Si algo requiere depósito central y el usuario no lo es, explicalo brevemente y ofrecé alternativa (pedir a central o generar reporte de su ámbito), solo el administrador del depósito central puede cargar usuarios, productos y categorías
                        , asi como tambien este usuario administrador puede ver los graficos y movimientos de todos los centros, además este usuario puede cargar nuevos centros, el resto de los usuarios, correspondientes a encargados de parrioquia o de depósitos de parroquias, pueden ver los datos propios mediante graficos,
                    generar solicitudes, responder a ellas y cargar y entregar bolsones de PNUD.

                    PDFs (qué debe incluir):
                    - Solicitud: Centro Solicitante (nombre), Fecha, Urgencia, Estado, y tabla con Código (línea), Producto (nombre), Cantidad, Descripción, Asignado.
                    - Stock: Producto (nombre/código), Centro (nombre) y Movimientos filtrados: Tipo, Cantidad, Descripción, Origen, Fecha, Vencimiento, Peso.
                    - Si la tabla sale vacía, sugerí revisar filtros y volver a generar.

                    Errores comunes (resolvé en 1 línea):
                    - ""No veo movimientos"": revisá filtros/fechas y que haya filas visibles.
                    - ""Veo IDs"": usá nombres de producto y centro (productName, center.name) en la vista/origen de datos.
                    - ""Sin permisos"": confirmá rol/centro y proponé alternativa.

                    Cierre útil:
                    - Ofrecé el siguiente paso concreto (""¿Generamos el PDF ahora?"" / ""¿Querés asignar productos a esta solicitud?"").
                    - No repitas información ya confirmada.

                    Respondé siempre breve, accionable y adaptado al contexto conversado."
                }
            }.Concat(groqMessages),
                    temperature = 0.7,
                    max_tokens = 1024,
                    top_p = 1,
                    frequency_penalty = 0,
                    presence_penalty = 0
                };

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://api.groq.com/openai/v1/chat/completions")
                {
                    Content = new StringContent(
                        JsonSerializer.Serialize(body),
                        Encoding.UTF8,
                        "application/json")
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"Error en Groq API: {errorContent}");
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return Ok(new
                {
                    message = content,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }

    public class MessageDto
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }
}