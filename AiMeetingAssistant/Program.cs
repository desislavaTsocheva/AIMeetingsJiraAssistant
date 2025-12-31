using AiMeetingAssistant.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Ollama;
using System.Net.Http;
using System.Text.Json;

var builder = Kernel.CreateBuilder();

builder.AddOllamaChatCompletion(
    modelId: "phi3",
    endpoint: new Uri("http://127.0.0.1:11434")
    );

builder.Plugins.AddFromType<MeetingPlugin>();
builder.Plugins.AddFromType<JiraPlugin>();

builder.Services.AddScoped<IJiraAiService, JiraMeetingService>();
var kernel = builder.Build();

string transcript = @"
Стефан: Трябва да завършим мобилното приложение до следващия вторник.
Елена: Аз ще отговарям за дизайна на началния екран и ще го пратя на Стефан до петък.
Георги: Добре, аз ще тествам логина и ще докладвам за бъгове.
Стефан: Супер, аз ще подготвя презентацията за клиента.";

string jsonPrompt = @"
Extract tasks from the meeting transcript.

Return ONLY a valid JSON array.
STRICT RULES:
- Do NOT include comments
- Do NOT include explanations
- Do NOT include markdown
- Do NOT include trailing text
- Use ONLY valid JSON (RFC 8259)

Each object MUST have exactly these keys:
- task (string)
- assignee (string)
- due_date (string)

Transcript:
{{$input}}
";


Console.WriteLine("=== AI MEETING ASSISTANT STARTED ===");
Console.WriteLine("AI Асистентът извлича задачи в JSON формат... Моля, изчакайте (може да отнеме 1-2 мин.)");

try
{ 
    var result = await kernel.InvokePromptAsync(jsonPrompt, new() { ["input"] = transcript });

    string rawJson = result.ToString().Trim();

    if (rawJson.Contains("```"))
    {
        var parts = rawJson.Split("```");
        rawJson = parts.Length > 1 ? parts[1].Replace("json", "").Trim() : parts[0].Trim();
    }

    Console.WriteLine("\n--- ПОЛУЧЕН JSON ---");
    Console.WriteLine(rawJson);

    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    rawJson = System.Text.RegularExpressions.Regex.Replace(
    rawJson,
    @"//.*?$",
    "",
    System.Text.RegularExpressions.RegexOptions.Multiline
);

    List<MeetingTask> taskList = JsonSerializer.Deserialize<List<MeetingTask>>(rawJson, options) ?? new();

    Console.WriteLine($"\n>>> Успешно разпознати {taskList.Count} задачи.");

    foreach (var t in taskList)
    {
        Console.WriteLine($"\nПращам към Jira: {t.task} (Отговорник: {t.assignee})");

        await kernel.InvokeAsync("JiraPlugin", "CreateJiraIssue", new()
        {
            ["summary"] = t.task,
            ["description"] = $"Отговорник: {t.assignee}, Срок: {t.due_date}"
        });
    }

    await kernel.InvokeAsync("MeetingPlugin", "ProcessTasksAndNotify", new() { ["markdownTable"] = rawJson });

    Console.WriteLine("\n>>> ЦЕЛИЯТ ПРОЦЕС ЗАВЪРШИ УСПЕШНО!");
}
catch (JsonException ex)
{
    Console.WriteLine($"\nГрешка при четене на JSON: {ex.Message}");
    Console.WriteLine("AI не върна чист JSON формат. Пробвайте пак.");
}
catch (Exception ex)
{
    Console.WriteLine($"\nОБЩА ГРЕШКА: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"ДЕТАЙЛИ: {ex.InnerException.Message}");
}

Console.ReadKey();

//public class MeetingTask
//{
//    public string task { get; set; } = string.Empty;
//    public string assignee { get; set; } = string.Empty;
//    public string due_date { get; set; } = string.Empty;
//}