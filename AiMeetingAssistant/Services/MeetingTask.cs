using System.Text.Json.Serialization;

public class MeetingTask
{
    [JsonPropertyName("task")] // Казва на C# да търси "task" в JSON-а
    public string task { get; set; } = "";

    [JsonPropertyName("assignee")]
    public string assignee { get; set; } = "";

    [JsonPropertyName("due_date")]
    public string due_date { get; set; } = "";

    // UI свойства
    public string Title { get => task; set => task = value; }
    public string Priority { get; set; } = "Medium";
}