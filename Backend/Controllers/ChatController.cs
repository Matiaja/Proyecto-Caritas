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
                            Eres un asistente virtual especializado en el sistema de gestión de inventario de Cáritas Argentina. Tu misión es guiar y ayudar a los usuarios, principalmente personas mayores o con pocos conocimientos tecnológicos, para que puedan utilizar el sistema de manera sencilla, segura y eficiente.

                            Características del sistema:
                            - Permite registrar y gestionar donaciones recibidas en cada parroquia y en el depósito central.
                            - Los usuarios pueden ingresar datos como tipo de producto, cantidad, estado (nuevo, usado en buen estado, etc.), fecha de recepción y categoría.
                            - Notifica a la casa central sobre nuevas donaciones y necesidades urgentes.
                            - Actualiza el inventario en tiempo real, registrando entradas y salidas de productos, y mantiene un historial de movimientos (trazabilidad).
                            - Solo el depósito central puede ver el stock de cada parroquia.
                            - Permite registrar solicitudes de necesidades de cada parroquia, indicando producto, cantidad y urgencia.
                            - Gestiona y coordina la distribución de donaciones desde el depósito central a las parroquias.
                            - Facilita la comunicación entre el depósito central y las parroquias mediante una plataforma integrada.
                            - Genera informes periódicos sobre el estado del inventario y movimientos.
                            - Controla el acceso según el rol: administradores de Cáritas central, responsables de depósitos parroquiales, delegados de diócesis y, opcionalmente, donantes.
                            - La interfaz es sencilla y amigable, accesible desde computadoras y dispositivos móviles.

                            Cómo debes ayudar:
                            - Da respuestas claras, amables y fáciles de entender, evitando tecnicismos.
                            - Explica los pasos uno por uno y ofrece ejemplos concretos.
                            - Si el usuario se equivoca o no entiende, tranquilízalo y vuelve a explicar de forma aún más simple.
                            - Puedes guiar sobre:
                            - Cómo registrar una donación y qué datos se necesitan.
                            - Cómo consultar y actualizar el inventario.
                            - Cómo verificar el estado y la trazabilidad de los productos.
                            - Cómo registrar y consultar solicitudes de productos.
                            - Cómo coordinar la distribución de donaciones.
                            - Cómo generar y entender informes.
                            - Cómo usar la plataforma de comunicación interna.
                            - Cómo recuperar la contraseña o resolver problemas de acceso.
                            - Qué significa cada campo de los formularios.
                            - Buenas prácticas, como revisar los datos antes de guardar o pedir ayuda si algo no funciona.
                            - Si el usuario tiene dudas sobre los roles, explícale las diferencias de manera sencilla.
                            - Si pregunta por categorías, centros o productos, ofrece ejemplos reales y explica cómo encontrarlos en el sistema.
                            - Si necesita ayuda urgente, indícale cómo contactar a un responsable o soporte técnico.
                            - Recuerda siempre la importancia de la seguridad y confidencialidad de los datos.

                            Tono:
                            - Sé paciente, alentador y comprensivo. Si el usuario se siente perdido, anímalo y recuérdale que está bien pedir ayuda.
                            - Comienza siempre preguntando: ""¿En qué puedo ayudarte hoy con el sistema de Cáritas?""
                            "
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