using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoCaritas.Models.DTOs;
using ProyectoCaritas.Services;

namespace ProyectoCaritas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        private readonly IPdfService _pdfService;

        public PdfController(IPdfService pdfService)
        {
            _pdfService = pdfService;
        }

        [Authorize]
        [HttpPost("generate")]
        public async Task<IActionResult> GeneratePdf([FromBody] PdfGenerationRequest request)
        {
            try
            {
                var pdfBytes = await _pdfService.GeneratePdfAsync(request);
                return File(pdfBytes, "application/pdf", $"{request.Title}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error generating PDF", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("stock-detail/{productId}/{centerId}")]
        public async Task<IActionResult> GenerateStockDetailPdf(int productId, int centerId)
        {
            try
            {
                // Aquí deberías obtener los datos reales del stock desde tu servicio
                var stockData = new List<dynamic>(); // Reemplazar con datos reales
                
                var pdfBytes = await _pdfService.GenerateStockDetailPdfAsync(productId, centerId, stockData);
                return File(pdfBytes, "application/pdf", $"stock_detail_{productId}_{centerId}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error generating stock PDF", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("request-detail/{requestId}")]
        public async Task<IActionResult> GenerateRequestDetailPdf(int requestId)
        {
            try
            {
                // Aquí deberías obtener los datos reales de la request desde tu servicio
                var requestData = new { }; // Reemplazar con datos reales
                
                var pdfBytes = await _pdfService.GenerateRequestDetailPdfAsync(requestId, requestData);
                return File(pdfBytes, "application/pdf", $"request_detail_{requestId}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error generating request PDF", error = ex.Message });
            }
        }
    }
}