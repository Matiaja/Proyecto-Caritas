namespace ProyectoCaritas.Models.DTOs
{
    public class PdfGenerationRequest
    {
        public string Title { get; set; } = "";
        public string? Subtitle { get; set; }
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public List<PdfSection> Sections { get; set; } = new();
        public PdfTableData? TableData { get; set; }
        public string? Footer { get; set; }

        public string? Orientation { get; set; } // "portrait" | "landscape"
        public List<string>? SignatureAreas { get; set; } // labels for signature lines

        // NEW: notes at right under the title
        public List<string>? RightNotes { get; set; }
    }

    public class PdfSection
    {
        public string Title { get; set; } = "";
        public string? Content { get; set; }
        public List<KeyValuePair<string, string>> KeyValuePairs { get; set; } = new();
    }

    public class PdfTableData
    {
        public string? Title { get; set; }
        public List<string> Headers { get; set; } = new();
        public List<List<string>> Rows { get; set; } = new();
    }
}