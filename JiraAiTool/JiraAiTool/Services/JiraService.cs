using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class JiraService
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string _projectKey;

    public JiraService(IConfiguration config)
    {
        _baseUrl = config["JiraSettings:BaseUrl"];
        _projectKey = config["JiraSettings:ProjectKey"] ?? "KAN";

        var email = config["JiraSettings:Email"];
        var token = config["JiraSettings:ApiToken"];

        _http = new HttpClient();

        var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{token}"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<bool> CreateIssueAsync(string summary, string description)
    {
        var payload = new
        {
            fields = new
            {
                project = new { key = _projectKey },
                summary = summary,
                description = new
                {
                    type = "doc",
                    version = 1,
                    content = new[]
                    {
                        new {
                            type = "paragraph",
                            content = new[] {
                                new { type = "text", text = description }
                            }
                        }
                    }
                },
                issuetype = new { name = "Task" }
            }
        };

        var json = JsonSerializer.Serialize(payload);
        Console.WriteLine("Sending to Jira:");
        Console.WriteLine(json);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync($"{_baseUrl}/rest/api/3/issue", content);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("JIRA ERROR:");
            Console.WriteLine(responseBody);
            return false;
        }

        Console.WriteLine("Jira issue created:");
        Console.WriteLine(responseBody);
        return true;
    }
}
