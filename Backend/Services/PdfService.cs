using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using ProyectoCaritas.Models.DTOs;

namespace ProyectoCaritas.Services
{
    public class PdfService : IPdfService
    {
        public async Task<byte[]> GeneratePdfAsync(PdfGenerationRequest request)
        {
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var yPoint = 40;

            var titleFont = new XFont("Arial", 18, XFontStyle.Bold);
            var subTitleFont = new XFont("Arial", 14, XFontStyle.Regular);
            var textFont = new XFont("Arial", 10, XFontStyle.Regular);
            var boldFont = new XFont("Arial", 10, XFontStyle.Bold);

            // Título
            gfx.DrawString(request.Title, titleFont, XBrushes.Black, new XRect(0, yPoint, page.Width, 20), XStringFormats.TopCenter);
            yPoint += 30;

            // Subtítulo
            if (!string.IsNullOrEmpty(request.Subtitle))
            {
                gfx.DrawString(request.Subtitle, subTitleFont, XBrushes.Gray, new XRect(0, yPoint, page.Width, 20), XStringFormats.TopCenter);
                yPoint += 25;
            }

            // Fecha de generación
            gfx.DrawString($"Generado el: {request.GeneratedDate:dd/MM/yyyy HH:mm}", textFont, XBrushes.Gray, new XRect(0, yPoint, page.Width - 40, 20), XStringFormats.TopRight);
            yPoint += 30;

            // Secciones
            foreach (var section in request.Sections)
            {
                yPoint = AddSection(gfx, page, section, yPoint, boldFont, textFont);
            }

            // Tabla
            if (request.TableData != null)
            {
                yPoint = AddTable(gfx, page, request.TableData, yPoint, textFont, boldFont);
            }

            // Footer
            if (!string.IsNullOrEmpty(request.Footer))
            {
                yPoint += 40;
                gfx.DrawString(request.Footer, textFont, XBrushes.Gray, new XRect(0, yPoint, page.Width, 20), XStringFormats.TopCenter);
            }

            // Guardar en memoria
            using var stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        }

        public async Task<byte[]> GenerateStockDetailPdfAsync(int productId, int centerId, List<dynamic> stockData)
        {
            var request = new PdfGenerationRequest
            {
                Title = "Detalle de Stock",
                Subtitle = $"Producto ID: {productId} - Centro ID: {centerId}",
                Sections = new List<PdfSection>
                {
                    new()
                    {
                        Title = "Información del Producto",
                        KeyValuePairs = new()
                        {
                            new("Producto ID", productId.ToString()),
                            new("Centro ID", centerId.ToString())
                        }
                    }
                },
                TableData = new PdfTableData
                {
                    Title = "Movimientos de Stock",
                    Headers = new List<string> { "Tipo", "Cantidad", "Descripción", "Origen", "Fecha", "Fecha Exp.", "Peso" },
                    Rows = stockData.Select(item => new List<string>
                    {
                        item.type?.ToString() ?? "-",
                        item.quantity?.ToString() ?? "-",
                        item.description?.ToString() ?? "-",
                        item.origin?.ToString() ?? "-",
                        ((DateTime?)item.date)?.ToString("dd/MM/yyyy") ?? "-",
                        ((DateTime?)item.expirationDate)?.ToString("dd/MM/yyyy") ?? "-",
                        item.weight?.ToString() ?? "-"
                    }).ToList()
                },
                Footer = $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}"
            };

            return await GeneratePdfAsync(request);
        }

        public async Task<byte[]> GenerateRequestDetailPdfAsync(int requestId, dynamic requestData)
        {
            var request = new PdfGenerationRequest
            {
                Title = "Detalle de Solicitud",
                Subtitle = $"Solicitud Nro. {requestId}",
                Sections = new List<PdfSection>
                {
                    new()
                    {
                        Title = "Información de la Solicitud",
                        KeyValuePairs = new List<KeyValuePair<string, string>>
                        {
                            new("Centro Solicitante", requestData.requestingCenter?.name?.ToString() ?? "-"),
                            new("Fecha", ((DateTime?)requestData.requestDate)?.ToString("dd/MM/yyyy") ?? "-"),
                            new("Nivel de Urgencia", requestData.urgencyLevel?.ToString() ?? "-")
                        }
                    }
                },
                Footer = $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}"
            };

            return await GeneratePdfAsync(request);
        }

        private int AddSection(XGraphics gfx, PdfPage page, PdfSection section, int y, XFont boldFont, XFont textFont)
        {
            y += 10;
            gfx.DrawString(section.Title, boldFont, XBrushes.Black, new XRect(20, y, page.Width - 40, 20), XStringFormats.TopLeft);
            y += 20;

            if (!string.IsNullOrEmpty(section.Content))
            {
                gfx.DrawString(section.Content, textFont, XBrushes.Black, new XRect(25, y, page.Width - 50, 20), XStringFormats.TopLeft);
                y += 20;
            }

            foreach (var kvp in section.KeyValuePairs)
            {
                gfx.DrawString($"{kvp.Key}: ", boldFont, XBrushes.Black, new XRect(25, y, 150, 20), XStringFormats.TopLeft);
                gfx.DrawString(kvp.Value, textFont, XBrushes.Black, new XRect(180, y, page.Width - 200, 20), XStringFormats.TopLeft);
                y += 18;
            }

            return y + 10;
        }

        private int AddTable(XGraphics gfx, PdfPage page, PdfTableData tableData, int y, XFont textFont, XFont headerFont)
        {
            y += 20;
            if (!string.IsNullOrEmpty(tableData.Title))
            {
                gfx.DrawString(tableData.Title, headerFont, XBrushes.Black, new XRect(20, y, page.Width - 40, 20), XStringFormats.TopLeft);
                y += 20;
            }

            const int colWidth = 70;
            int startX = 20;

            // Headers
            for (int i = 0; i < tableData.Headers.Count; i++)
            {
                gfx.DrawString(tableData.Headers[i], headerFont, XBrushes.Black, new XRect(startX + i * colWidth, y, colWidth, 20), XStringFormats.TopLeft);
            }
            y += 20;

            // Rows
            foreach (var row in tableData.Rows)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    gfx.DrawString(row[i], textFont, XBrushes.Black, new XRect(startX + i * colWidth, y, colWidth, 20), XStringFormats.TopLeft);
                }
                y += 18;
            }

            return y + 10;
        }
    }
}
