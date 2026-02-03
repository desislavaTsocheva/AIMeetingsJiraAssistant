using DocumentFormat.OpenXml.Packaging;
using NAudio.Wave;
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
        using var outputStream = new MemoryStream();

        try
        {
            using (var reader = new StreamMediaFoundationReader(stream))
            {
                var outFormat = new WaveFormat(16000, 1); 
                using (var resampler = new MediaFoundationResampler(reader, outFormat))
                {
                    resampler.ResamplerQuality = 60; 
                    WaveFileWriter.WriteWavFileToStream(outputStream, resampler);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Audio Conversion Error: {ex.Message}");
            return "Audio file compilation error";
        }

        outputStream.Position = 0;
        var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "models", "ggml-base.bin");

        if (!File.Exists(modelPath))
        {
            return "Model ggml-base.bin missing in wwwroot/models/";
        }

        using var factory = WhisperFactory.FromPath(modelPath);
        using var processor = factory.CreateBuilder()
            .WithLanguage("bg")
            .Build();

        var sb = new System.Text.StringBuilder();

        await foreach (var segment in processor.ProcessAsync(outputStream))
        {
            sb.Append(segment.Text).Append(" ");
        }

        return sb.ToString().Trim();
    }
}
