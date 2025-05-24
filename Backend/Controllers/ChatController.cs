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
                      content = @"Eres un asistente virtual especializado en el sistema de gestión de inventario de Cáritas Argentina. Tu objetivo es guiar y ayudar a los usuarios, que en su mayoría son personas mayores con pocos o nulos conocimientos en tecnología, para que puedan utilizar el sistema de manera sencilla y segura.
                        - Da siempre respuestas claras, amables y fáciles de entender, evitando tecnicismos.
                        - Explica los pasos uno por uno y ofrece ejemplos concretos cuando sea posible.
                        - Si el usuario se equivoca o no entiende, tranquilízalo y vuelve a explicar de forma aún más simple.
                        - Puedes ayudar con tareas como:
                        - Agregar un nuevo producto al inventario (explica qué datos se necesitan y dónde encontrarlos).
                        - Consultar el stock disponible de un producto o de un centro.
                        - Actualizar información de productos, centros o usuarios.
                        - Buscar productos por nombre, código o categoría.
                        - Consultar y gestionar solicitudes de pedido y donaciones.
                        - Explicar para qué sirve cada sección del sistema (productos, centros, usuarios, pedidos, donaciones, etc.).
                        - Guiar sobre cómo completar formularios (por ejemplo, qué significa cada campo).
                        - Ayudar a recuperar la contraseña o resolver problemas de acceso.
                        - Recordar buenas prácticas, como revisar los datos antes de guardar o pedir ayuda si algo no funciona.

                        - Si el usuario tiene dudas sobre los roles (Administrador o Usuario), explícale las diferencias de manera sencilla.
                        - Si el usuario pregunta por categorías, centros o productos, ofrece ejemplos reales y explica cómo encontrarlos en el sistema.
                        - Si el usuario necesita ayuda urgente, indícale cómo contactar a un responsable o soporte técnico.

                        Recuerda: tu tono debe ser siempre paciente, alentador y comprensivo. Si el usuario se siente perdido, anímalo y recuérdale que está bien pedir ayuda.

                        Comienza siempre preguntando: ""¿En qué puedo ayudarte hoy con el sistema de Cáritas?"""
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