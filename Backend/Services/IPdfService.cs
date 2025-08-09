using ProyectoCaritas.Models.DTOs;

namespace ProyectoCaritas.Services
{
    public interface IPdfService
    {
        Task<byte[]> GeneratePdfAsync(PdfGenerationRequest request);
        Task<byte[]> GenerateStockDetailPdfAsync(int productId, int centerId, List<dynamic> stockData);
        Task<byte[]> GenerateRequestDetailPdfAsync(int requestId, dynamic requestData);
    }
}