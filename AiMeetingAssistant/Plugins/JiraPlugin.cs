using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.SemanticKernel;

public class JiraPlugin
{
    private readonly string _domain = "tsochevadesislava"; 
    private readonly string _email = "tsochevadesislava@gmail.com";
    private readonly string _apiToken = "ATATT3xFfGF00i4e1y1ghhllQ1yi_RiKkRmXs88US1l58Bhmvn-mHmUyP2Pn137NkTYyG0Myn9yi_vl3sLrgb5QkTrRTnLnuu_UFdpTkMjDS2NabpzJiPiXlJPbhsAo3XKx-lsi3hNL8OzZQjUpyOz4aIw3B3tiLc-reFIpu37khgwuXSwBHq-A=6AB0C7D2";
    private readonly string _projectKey = "KAN"; 

    [KernelFunction]
    [Description("Създава нов тикет в Jira.")]
    public async Task CreateJiraIssue(string summary, string description)
    {
        string url = $"https://{_domain}.atlassian.net/rest/api/2/issue";

        using var client = new HttpClient();

        var authBytes = Encoding.ASCII.GetBytes($"{_email}:{_apiToken}");
        var base64Auth = Convert.ToBase64String(authBytes);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);

        var issueData = new
        {
            fields = new
            {
                project = new { key = _projectKey },
                summary = summary,
                description = description,
                issuetype = new { name = "Task" } 
            }
        };

        var json = JsonSerializer.Serialize(issueData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        Console.WriteLine($"Опит за създаване на задача: {summary}...");

        var response = await client.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Успешно създаден тикет!");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Грешка: {response.StatusCode} - {error}");
        }
    }
}