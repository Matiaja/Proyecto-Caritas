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
                    content = "Eres un asistente especializado en el sistema de gestión de inventario de Cáritas. " +
                             "Proporciona respuestas claras, concisas y útiles."
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