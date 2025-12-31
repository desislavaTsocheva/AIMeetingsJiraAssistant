using AiMeetingAssistant.Services;
using Microsoft.SemanticKernel;
using System.Text.Json;
using System.Text.RegularExpressions;


public class JiraMeetingService : IJiraAiService
{
    private readonly Kernel _kernel;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };


    public JiraMeetingService()
    {
        var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) }; // Даваме му 5 минути
        var builder = Kernel.CreateBuilder();
        builder.AddOllamaChatCompletion(
           modelId: "llama3.2:1b",
            endpoint: new Uri("http://127.0.0.1:11434")
        );
        builder.Plugins.AddFromType<MeetingPlugin>();
        builder.Plugins.AddFromType<JiraPlugin>();

        _kernel = builder.Build();
    }

    public async Task<List<MeetingTask>> ProcessTranscriptionAsync(string transcript)
    {
        string jsonPrompt = @"
[INST] Extract all tasks/actions from the text.
Return ONLY a valid JSON array.
Keys: ""task"", ""assignee"", ""due_date"".

Example Input: Стефан да направи дизайн до утре.
Example Output: [{""task"": ""Дизайн"", ""assignee"": ""Стефан"", ""due_date"": ""утре""}]

Text:
{{$input}}
[/INST]
JSON:";

        var result = await _kernel.InvokePromptAsync(jsonPrompt, new() { ["input"] = transcript });
        string rawJson = CleanJson(result.ToString());

        if (string.IsNullOrWhiteSpace(rawJson) || !rawJson.StartsWith("["))
        {
            // Връщаме празен списък, вместо да гърмим с грешка
            return new List<MeetingTask>();
        }
        try
        {
            return JsonSerializer.Deserialize<List<MeetingTask>>(rawJson, _jsonOptions) ?? new();
        }
        catch
        {
            // Ако десериализацията се провали, опитай да добавиш скоби ръчно
            try
            {
                return JsonSerializer.Deserialize<List<MeetingTask>>("[" + rawJson + "]", _jsonOptions) ?? new();
            }
            catch { return new List<MeetingTask>(); }
        }
        //return JsonSerializer.Deserialize<List<MeetingTask>>(rawJson, _jsonOptions) ?? new();
    }

    public async Task<bool> SendToJiraAsync(List<MeetingTask> tasks)
    {
        try
        {
            foreach (var task in tasks)
            {
                await _kernel.InvokeAsync("JiraPlugin", "CreateJiraIssue", new()
                {
                    ["summary"] = task.task,
                    ["description"] = $"Отговорник: {task.assignee}, Срок: {task.due_date}"
                });
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string CleanJson(string rawJson)
    {
        rawJson = rawJson.Trim();

        // 1. Търсим къде започва и свършва масивът
        int startIndex = rawJson.IndexOf('[');
        int endIndex = rawJson.LastIndexOf(']');

        if (startIndex != -1 && endIndex != -1 && endIndex > startIndex)
        {
            rawJson = rawJson.Substring(startIndex, (endIndex - startIndex) + 1);
        }

        // 2. Премахваме Markdown тагове, ако моделът ги е сложил
        rawJson = rawJson.Replace("```json", "").Replace("```", "").Trim();

        // 3. Премахваме коментари (твоята текуща логика)
        return Regex.Replace(rawJson, @"//.*?$", "", RegexOptions.Multiline);
    }
}