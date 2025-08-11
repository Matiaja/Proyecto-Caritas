using PdfSharpCore;
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

            // A4 y orientación
            page.Size = PageSize.A4;
            if (string.Equals(request.Orientation, "landscape", StringComparison.OrdinalIgnoreCase))
                page.Orientation = PageOrientation.Landscape;

            var gfx = XGraphics.FromPdfPage(page);

            // márgenes
            const int margin = 20;
            const int topMargin = 40;
            const int bottomMargin = 40;

            var yPoint = topMargin;

            var titleFont = new XFont("Arial", 18, XFontStyle.Bold);
            var subTitleFont = new XFont("Arial", 14, XFontStyle.Regular);
            var textFont = new XFont("Arial", 10, XFontStyle.Regular);
            var boldFont = new XFont("Arial", 10, XFontStyle.Bold);

            // Título (mayúsculas) con ajuste al ancho de A4
            var titleText = (request.Title ?? string.Empty).ToUpperInvariant();
            var (titleFit, titleFitFont) = FitText(gfx, titleText, titleFont, page.Width - (margin * 2), minSize: 9);
            gfx.DrawString(titleFit, titleFitFont, XBrushes.Black, new XRect(margin, yPoint, page.Width - (margin * 2), 24), XStringFormats.TopCenter);

            yPoint += 28; // espacio bajo título

            // RightNotes bajo el título, a la derecha, sin superponer
            if (request.RightNotes != null && request.RightNotes.Count > 0)
            {
                int yn = yPoint;
                var rightNoteFont = new XFont("Arial", 10, XFontStyle.Bold);
                foreach (var raw in request.RightNotes)
                {
                    var noteText = (raw ?? string.Empty).ToUpperInvariant();
                    gfx.DrawString(noteText, rightNoteFont, XBrushes.Black, new XRect(margin, yn, page.Width - (margin * 2), 14), XStringFormats.TopRight);
                    yn += 16;
                }
                yPoint = yn + 6; // deja margen extra después de las notas
            }

            // Subtítulo
            if (!string.IsNullOrEmpty(request.Subtitle))
            {
                // ajusta subtítulo si es largo
                var (subFit, subFont) = FitText(gfx, request.Subtitle, subTitleFont, page.Width - (margin * 2), minSize: 9);
                gfx.DrawString(subFit, subFont, XBrushes.Gray, new XRect(margin, yPoint, page.Width - (margin * 2), 20), XStringFormats.TopCenter);
                yPoint += 24;
            }

            // Fecha arriba a la derecha
            gfx.DrawString($"Generado el: {request.GeneratedDate:dd/MM/yyyy HH:mm}", textFont, XBrushes.Gray, new XRect(margin, yPoint, page.Width - (margin * 2), 16), XStringFormats.TopRight);
            yPoint += 24;

            // Secciones (básicas, sin paginación avanzada)
            if (request.Sections != null)
            {
                for (int si = 0; si < request.Sections.Count; si++)
                {
                    var section = request.Sections[si];

                    // Si la sección indica render side-by-side con la siguiente y existe la siguiente
                    if (section.SideBySideWithNext && si + 1 < request.Sections.Count)
                    {
                        var nextSection = request.Sections[si + 1];
                        yPoint = AddSectionsSideBySide(gfx, page, section, nextSection, yPoint, boldFont, textFont);
                        si++; // saltar la siguiente porque ya fue renderizada en paralelo
                    }
                    else
                    {
                        yPoint = AddSection(gfx, page, section, yPoint, boldFont, textFont);
                    }
                }
            }

            // Tabla con paginado y centrado
            if (request.TableData != null)
            {
                yPoint = AddTable(document, request.Orientation, ref page, ref gfx, request.TableData, yPoint, textFont, boldFont, topMargin, bottomMargin, margin);
            }

            // Firmas SOLO en la última página
            if (request.SignatureAreas != null && request.SignatureAreas.Count > 0)
            {
                yPoint = DrawSignatureAreas(gfx, page, yPoint, request.SignatureAreas, textFont);
            }

            // Footer de texto
            if (!string.IsNullOrEmpty(request.Footer))
            {
                gfx.DrawString(request.Footer, textFont, XBrushes.Gray, new XRect(margin, page.Height - 30, page.Width - (margin * 2), 16), XStringFormats.Center);
            }

            using var stream = new MemoryStream();
            document.Save(stream, false);
            return stream.ToArray();
        }

        // Crea una nueva página manteniendo orientación y devuelve gfx
        private void NewPage(PdfDocument document, string? orientation, ref PdfPage page, ref XGraphics gfx)
        {
            page = document.AddPage();
            page.Size = PageSize.A4;
            if (string.Equals(orientation, "landscape", StringComparison.OrdinalIgnoreCase))
                page.Orientation = PageOrientation.Landscape;

            gfx.Dispose();
            gfx = XGraphics.FromPdfPage(page);
        }

        private int AddTable(
            PdfDocument document,
            string? orientation,
            ref PdfPage page,
            ref XGraphics gfx,
            PdfTableData tableData,
            int y,
            XFont textFont,
            XFont headerFont,
            int topMargin,
            int bottomMargin,
            int sideMargin)
        {
            double availableWidth = page.Width - (sideMargin * 2);
            int cols = tableData.Headers.Count;
            if (cols == 0) return y;

            double colWidth = availableWidth / cols;
            double headerHeight = 24;
            double rowHeight = 22;
            const double pad = 4;

            // Título de la tabla (ajuste + clip)
            if (!string.IsNullOrEmpty(tableData.Title))
            {
                var (fit, font) = FitText(gfx, tableData.Title!, headerFont, availableWidth, minSize: 8);
                var tRect = new XRect(sideMargin, y, availableWidth, 18);
                DrawClippedString(gfx, fit, font, XBrushes.Black, tRect, XStringFormats.TopLeft);
                y += 20;
            }

            // Nueva página si no entra el header
            if (y + headerHeight > page.Height - bottomMargin)
            {
                NewPage(document, orientation, ref page, ref gfx);
                y = topMargin;
            }

            // Encabezado
            y = DrawTableHeader(gfx, y, sideMargin, availableWidth, headerHeight, colWidth, cols, tableData.Headers, headerFont, pad);

            // Filas con salto de página
            foreach (var row in tableData.Rows)
            {
                if (y + rowHeight > page.Height - bottomMargin)
                {
                    NewPage(document, orientation, ref page, ref gfx);
                    y = topMargin;
                    y = DrawTableHeader(gfx, y, sideMargin, availableWidth, headerHeight, colWidth, cols, tableData.Headers, headerFont, pad);
                }

                y = DrawTableRow(gfx, y, sideMargin, colWidth, rowHeight, cols, row, textFont, pad);
            }

            return y + 8;
        }

        // Header de tabla (helper normal, no función local)
        private int DrawTableHeader(
            XGraphics gfx,
            int y,
            double sideMargin,
            double availableWidth,
            double headerHeight,
            double colWidth,
            int cols,
            IList<string> headers,
            XFont headerFont,
            double pad)
        {
            var headerRect = new XRect(sideMargin, y, availableWidth, headerHeight);
            gfx.DrawRectangle(new XSolidBrush(XColor.FromArgb(230, 230, 230)), headerRect);
            gfx.DrawRectangle(XPens.Black, headerRect);

            for (int c = 0; c < cols; c++)
            {
                var cellRect = new XRect(sideMargin + c * colWidth, y, colWidth, headerHeight);
                gfx.DrawRectangle(XPens.Black, cellRect);

                var allowed = cellRect.Width - pad * 2;
                var (hdrText, hdrFont) = FitText(gfx, headers[c] ?? string.Empty, headerFont, allowed, minSize: 8);
                var inner = new XRect(cellRect.X + pad, cellRect.Y, allowed, headerHeight);
                DrawClippedString(gfx, hdrText, hdrFont, XBrushes.Black, inner, XStringFormats.Center);
            }

            return y + (int)headerHeight;
        }

        // Fila de tabla (helper normal, no función local)
        private int DrawTableRow(
            XGraphics gfx,
            int y,
            double sideMargin,
            double colWidth,
            double rowHeight,
            int cols,
            IReadOnlyList<string> row,
            XFont textFont,
            double pad)
        {
            for (int c = 0; c < cols; c++)
            {
                var text = (c < row.Count ? row[c] : string.Empty) ?? string.Empty;
                var cellRect = new XRect(sideMargin + c * colWidth, y, colWidth, rowHeight);
                gfx.DrawRectangle(XPens.Black, cellRect);

                var allowed = cellRect.Width - pad * 2;
                var (cellText, cellFont) = FitText(gfx, text, textFont, allowed, minSize: 8);
                var inner = new XRect(cellRect.X + pad, cellRect.Y, allowed, rowHeight);
                DrawClippedString(gfx, cellText, cellFont, XBrushes.Black, inner, XStringFormats.Center);
            }

            return y + (int)rowHeight;
        }

        private int DrawSignatureAreas(XGraphics gfx, PdfPage page, int y, List<string> labels, XFont textFont)
        {
            var bottomAreaTop = (int)page.Height - 120;
            y = Math.Max(y + 10, bottomAreaTop);

            int margin = 20;
            int totalWidth = (int)page.Width - (margin * 2);
            int per = labels.Count > 0 ? totalWidth / labels.Count : totalWidth;

            for (int i = 0; i < labels.Count; i++)
            {
                int x = margin + (i * per);
                int lineY = (int)page.Height - 60;

                gfx.DrawLine(XPens.Black, x + 20, lineY, x + per - 20, lineY);
                gfx.DrawString(labels[i], textFont, XBrushes.Gray, new XRect(x, lineY + 5, per, 18), XStringFormats.TopCenter);
            }
            return (int)page.Height - 40;
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
            y += 8;
            gfx.DrawString(section.Title, boldFont, XBrushes.Black, new XRect(20, y, page.Width - 40, 18), XStringFormats.TopLeft);
            y += 18;

            if (!string.IsNullOrEmpty(section.Content))
            {
                gfx.DrawString(section.Content, textFont, XBrushes.Black, new XRect(25, y, page.Width - 50, 18), XStringFormats.TopLeft);
                y += 18;
            }

            foreach (var kvp in section.KeyValuePairs)
            {
                gfx.DrawString($"{kvp.Key}:", boldFont, XBrushes.Black, new XRect(25, y, 150, 16), XStringFormats.TopLeft);
                gfx.DrawString(kvp.Value ?? string.Empty, textFont, XBrushes.Black, new XRect(180, y, page.Width - 200, 16), XStringFormats.TopLeft);
                y += 16;
            }

            return y + 8;
        }

        private int AddSectionsSideBySide(
            XGraphics gfx,
            PdfPage page,
            PdfSection left,
            PdfSection right,
            int y,
            XFont boldFont,
            XFont textFont)
        {
            const int margin = 20;
            const int gap = 20; // espacio entre columnas
            double pageWidth = page.Width;
            double availableWidth = pageWidth - (margin * 2);
            double colWidth = (availableWidth - gap) / 2;

            // Coordenadas iniciales para cada columna
            double leftX = margin;
            double rightX = margin + colWidth + gap;

            double curYLeft = y;
            double curYRight = y;

            // Espacio superior
            curYLeft += 8;
            curYRight += 8;

            // Dibujar títulos de cada columna (si existen), y ajustar tamaño si es necesario
            if (!string.IsNullOrEmpty(left.Title))
            {
                var (txtLeftTitle, fLeftTitle) = FitText(gfx, left.Title!, boldFont, colWidth);
                DrawClippedString(gfx, txtLeftTitle, fLeftTitle, XBrushes.Black, new XRect(leftX, curYLeft, colWidth, fLeftTitle.Size + 4), XStringFormats.TopLeft);
                curYLeft += (int)fLeftTitle.Size + 6;
            }

            if (!string.IsNullOrEmpty(right.Title))
            {
                var (txtRightTitle, fRightTitle) = FitText(gfx, right.Title!, boldFont, colWidth);
                DrawClippedString(gfx, txtRightTitle, fRightTitle, XBrushes.Black, new XRect(rightX, curYRight, colWidth, fRightTitle.Size + 4), XStringFormats.TopLeft);
                curYRight += (int)fRightTitle.Size + 6;
            }

            // Ahora dibujamos key-value pairs en cada columna por separado:
            // Para consistencia usamos keyWidth fijo dentro de la columna.
            double keyWidth = Math.Min(120, colWidth * 0.4); // ancho para la clave
            double valueWidth = colWidth - keyWidth - 6;
            double lineHeight = textFont.Size + 4;

            // Left column key-values
            if (left.KeyValuePairs != null)
            {
                foreach (var kv in left.KeyValuePairs)
                {
                    // Key (negrita)
                    var keyRect = new XRect(leftX, curYLeft, keyWidth, lineHeight);
                    var (keyText, keyFont) = FitText(gfx, (kv.Key ?? string.Empty) + ":", boldFont, keyWidth);
                    DrawClippedString(gfx, keyText, keyFont, XBrushes.Black, keyRect, XStringFormats.TopLeft);

                    // Value
                    var valRect = new XRect(leftX + keyWidth + 6, curYLeft, valueWidth, lineHeight * 3);
                    var (valText, valFont) = FitText(gfx, kv.Value ?? string.Empty, textFont, valueWidth, minSize: 7);
                    DrawClippedString(gfx, valText, valFont, XBrushes.Black, valRect, XStringFormats.TopLeft);

                    curYLeft += (int)lineHeight;
                }
            }

            // Right column key-values
            if (right.KeyValuePairs != null)
            {
                foreach (var kv in right.KeyValuePairs)
                {
                    var keyRect = new XRect(rightX, curYRight, keyWidth, lineHeight);
                    var (keyText, keyFont) = FitText(gfx, (kv.Key ?? string.Empty) + ":", boldFont, keyWidth);
                    DrawClippedString(gfx, keyText, keyFont, XBrushes.Black, keyRect, XStringFormats.TopLeft);

                    var valRect = new XRect(rightX + keyWidth + 6, curYRight, valueWidth, lineHeight * 3);
                    var (valText, valFont) = FitText(gfx, kv.Value ?? string.Empty, textFont, valueWidth, minSize: 7);
                    DrawClippedString(gfx, valText, valFont, XBrushes.Black, valRect, XStringFormats.TopLeft);

                    curYRight += (int)lineHeight;
                }
            }

            // calcular la altura usada por ambas columnas y devolver la nueva Y (con un pequeño padding)
            double usedHeight = Math.Max(curYLeft, curYRight) - y;
            return y + (int)usedHeight + 8;
        }


        // Ajusta el tamaño de fuente hasta un mínimo y si aún no entra, trunca con "…"
        private (string text, XFont font) FitText(XGraphics gfx, string text, XFont baseFont, double maxWidth, double minSize = 7, double step = 0.5)
        {
            if (string.IsNullOrEmpty(text)) return (string.Empty, baseFont);

            var size = baseFont.Size;
            XFont font = baseFont;

            while (size > minSize)
            {
                font = new XFont(baseFont.FontFamily.Name, size, baseFont.Style);
                var w = gfx.MeasureString(text, font).Width;
                if (w <= maxWidth) return (text, font);
                size -= step;
            }

            // Truncar con elipsis
            string ellipsis = "…";
            string t = text;
            while (t.Length > 0 && gfx.MeasureString(t + ellipsis, font).Width > maxWidth)
                t = t[..^1];

            return (t.Length == 0 ? string.Empty : t + ellipsis, font);
        }

        // Dibuja texto recortado en un rectángulo, sin romper palabras
        private void DrawClippedString(XGraphics gfx, string text, XFont font, XBrush brush, XRect rect, XStringFormat format)
        {
            // Palabras en el texto
            var words = text.Split(' ');
            string line = string.Empty;
            double y = rect.Y;

            foreach (var word in words)
            {
                // Probar con la nueva palabra
                var testLine = line + (line.Length > 0 ? " " : "") + word;
                var size = gfx.MeasureString(testLine, font);

                // Si no cabe, dibujar la línea anterior y empezar una nueva
                if (size.Width > rect.Width)
                {
                    if (line.Length > 0)
                    {
                        gfx.DrawString(line, font, brush, new XRect(rect.X, y, rect.Width, rect.Height), format);
                        y += font.Size + 2; // Espacio entre líneas
                    }
                    line = word; // Nueva línea comienza con la palabra actual
                }
                else
                {
                    line = testLine; // La palabra cabe, así que actualiza la línea de texto
                }
            }

            // Dibujar la última línea si no está vacía
            if (line.Length > 0)
            {
                gfx.DrawString(line, font, brush, new XRect(rect.X, y, rect.Width, rect.Height), format);
            }
        }
    }
}
