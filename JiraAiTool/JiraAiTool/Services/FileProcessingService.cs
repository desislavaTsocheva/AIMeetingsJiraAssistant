using DocumentFormat.OpenXml.Packaging;
using UglyToad.PdfPig;
using Whisper.net;

public class FileProcessingService
{
    public async Task<string> ExtractTextFromPdf(Stream stream)
    {
        stream.Position = 0;
        using var pdf = PdfDocument.Open(stream);
        return string.Join(" ", pdf.GetPages().Select(p => p.Text));
    }

    public string ExtractTextFromWord(Stream stream)
    {
        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        return doc.MainDocumentPart!.Document.Body!.InnerText;
    }

    public async Task<string> TranscribeAudio(Stream stream)
    {
        stream.Position = 0;

        var modelPath = "wwwroot/models/ggml-base.bin";

        using var factory = WhisperFactory.FromPath(modelPath);
        using var processor = factory.CreateBuilder()
            .WithLanguage("bg")
            .Build();

        var sb = new System.Text.StringBuilder();

        await foreach (var segment in processor.ProcessAsync(stream))
        {
            sb.Append(segment.Text).Append(" ");
        }

        return sb.ToString();
    }
}
