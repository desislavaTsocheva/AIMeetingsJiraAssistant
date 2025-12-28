using System.ComponentModel;
using Microsoft.SemanticKernel;

public class MeetingPlugin
{
    private readonly Dictionary<string, string> _employeeDirectory = new()
    {
        ["Стефан"] = "stefan@company.com",
        ["Елена"] = "elena@company.com",
        ["Георги"] = "georgi@company.com",
        ["Мария"] = "maria@company.com"
    };

    [KernelFunction]
    [Description("Генерира професионален Markdown отчет и проверява имейлите.")]
    public async Task ProcessTasksAndNotify(string markdownTable)
    {
        string fileName = "MEETING_REPORT.md";

        string content = "Протокол от среща\n";
        content += $"Дата на генериране: {DateTime.Now:dd.MM.yyyy HH:mm}\n\n";
        content += "Извлечени задачи\n";
        content += markdownTable + "\n\n";

        content += "Контакти за уведомяване\n";

        bool foundAny = false;
        foreach (var employee in _employeeDirectory)
        {
            if (markdownTable.Contains(employee.Key))
            {
                content += $"- [ ] **{employee.Key}**: {employee.Value} (Известието е подготвено)\n";
                foundAny = true;
            }
        }

        if (!foundAny) content += "_Няма открити съвпадения в речника със служители._\n";

        await File.WriteAllTextAsync(fileName, content);
        Console.WriteLine($"\n[Система]: Markdown отчетът е генериран успешно: {fileName}");
    }
}