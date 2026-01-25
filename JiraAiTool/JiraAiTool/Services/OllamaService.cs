using JiraAiTool.Models;
using OllamaSharp;
using System.Text.Json;

public class OllamaService
{
    private readonly OllamaApiClient _ollama;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public OllamaService()
    {
        _ollama = new OllamaApiClient("http://localhost:11434");
        _ollama.SelectedModel = "llama3:8b";
    }

    public async Task<List<TaskItem>> ExtractTasksAsync(string transcript)
    {
        string prompt = $@"
        Analyze the meeting transcript and extract all tasks.
        Return ONLY a valid JSON array of objects. 
        Do not include markdown formatting like ```json.

        Each object MUST have these exact keys:
        - ""Description"": (The action to be done)
        - ""AssigneeName"": (Who should do it). For 'AssigneeName', try to find the full name of the person mentioned as responsible.
        - ""Deadline"": (Date in YYYY-MM-DD or null if not mentioned)

        Text to analyze:
        {transcript}";

        try
        {
            string response = "";
            await foreach (var stream in _ollama.GenerateAsync(prompt))
            {
                response += stream.Response;
            }

            string cleanJson = CleanJson(response);

            if (string.IsNullOrWhiteSpace(cleanJson))
            {
                Console.WriteLine("AI returned empty or invalid JSON");
                return new List<TaskItem>();
            }

            var tasks = JsonSerializer.Deserialize<List<TaskItem>>(cleanJson, _jsonOptions);
            return tasks ?? new List<TaskItem>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AI Error: {ex.Message}");
            return new List<TaskItem>();
        }
    }

    private string CleanJson(string input)
    {
        int start = input.IndexOf('[');
        int end = input.LastIndexOf(']');
        if (start == -1 || end == -1) return "";
        return input.Substring(start, (end - start) + 1);
    }
}