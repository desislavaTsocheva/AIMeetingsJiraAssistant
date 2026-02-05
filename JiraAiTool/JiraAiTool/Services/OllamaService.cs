using JiraAiTool.Models;
using JiraAiTool.Services;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models;
using System.Text;
using System.Text.Json;

public class OllamaService
{
    private readonly OllamaApiClient _ollama;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public OllamaService(string endpoint = "http://localhost:11434", string model = "llama3:8b")
    {
        _ollama = new OllamaApiClient(endpoint);
        _ollama.SelectedModel = model;
    }

    public async Task<List<TaskItem>> ExtractTasksAsync(
        string transcript,
        List<JiraService.JiraUser> jiraUsers,
        CancellationToken ct = default) 
    {
        var usersText = string.Join(", ", jiraUsers.Select(u => u.displayName));

        string prompt = $@"
            Analyze the meeting transcript and extract all tasks.
            Return ONLY a valid JSON array of objects. 
            Do not include markdown formatting like ```json.

            Each object MUST have these exact keys:
            - ""Description"": Analyze the action based on the transcript and provide additional instructions for efficiency.
            - ""Title"": Based on the description write short title for the task. 
            - ""AssigneeName"": Match exactly with one of these names: {usersText}. If no match, return the most likely name.
            - ""Deadline"": Date in 2026-MM-DD or null.
            - ""Priority"": 'High' if deadline is soon or requested, otherwise 'Medium'.
            -""StartDate"": Take the ugc datetime now in YYYY-MM-DD. 
            Text to analyze:
            {transcript}";

        try
        {
            var responseBuilder = new StringBuilder();

            var request = new GenerateRequest
            {
                Prompt = prompt,
                Model = _ollama.SelectedModel 
            };

            await foreach (var stream in _ollama.GenerateAsync(request, ct))
            {
                if (stream != null)
                {
                    responseBuilder.Append(stream.Response);
                }
            }

            string fullResponse = responseBuilder.ToString();
            string cleanJson = CleanJson(fullResponse);

            if (string.IsNullOrWhiteSpace(cleanJson))
            {
                return new List<TaskItem>();
            }

            return JsonSerializer.Deserialize<List<TaskItem>>(cleanJson, _jsonOptions) ?? new List<TaskItem>();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("AI Task extraction was cancelled.");
            return new List<TaskItem>();
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

        if (start == -1 || end == -1 || end < start)
            return string.Empty;

        return input.Substring(start, (end - start) + 1);
    }
}