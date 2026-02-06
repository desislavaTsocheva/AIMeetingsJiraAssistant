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
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.tmp");

        try
        {
            using (var fs = File.OpenWrite(tempFile))
            {
                await stream.CopyToAsync(fs);
            }

            using var reader = new MediaFoundationReader(tempFile);

            var outFormat = new WaveFormat(16000, 1);

            using var wavStream = new MemoryStream();
            using (var resampler = new MediaFoundationResampler(reader, outFormat))
            {
                resampler.ResamplerQuality = 60;
                WaveFileWriter.WriteWavFileToStream(wavStream, resampler);
            }

            wavStream.Position = 0;

            var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "models", "ggml-base.bin");

            using var factory = WhisperFactory.FromPath(modelPath);
            using var processor = factory.CreateBuilder()
                .WithLanguageDetection()
                .Build();

            var sb = new System.Text.StringBuilder();
            await foreach (var segment in processor.ProcessAsync(wavStream))
            {
                sb.Append(segment.Text).Append(" ");
            }

            return sb.ToString().Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Audio Error: {ex.Message}");
            return "";
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

}
