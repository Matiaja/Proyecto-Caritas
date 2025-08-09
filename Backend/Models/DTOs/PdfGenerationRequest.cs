namespace ProyectoCaritas.Models.DTOs
{
    public class PdfGenerationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public List<PdfSection> Sections { get; set; } = new();
        public PdfTableData? TableData { get; set; }
        public string? LogoBase64 { get; set; }
        public string? Footer { get; set; }
    }

    public class PdfSection
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<KeyValuePair<string, string>> KeyValuePairs { get; set; } = new();
    }

    public class PdfTableData
    {
        public List<string> Headers { get; set; } = new();
        public List<List<string>> Rows { get; set; } = new();
        public string? Title { get; set; }
    }
}