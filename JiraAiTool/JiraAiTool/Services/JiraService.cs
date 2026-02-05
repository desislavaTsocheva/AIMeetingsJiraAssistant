using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace JiraAiTool.Services;

public class JiraService
{
    private readonly IHttpClientFactory _factory;
    private readonly UserSession _session;
    private readonly string _projectKey;
    private readonly AtlassianAuthService _authService;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public JiraService(IHttpClientFactory factory, UserSession session, AtlassianAuthService authService, IConfiguration config)
    {
        _factory = factory;
        _session = session;
        _authService = authService;
        _projectKey = config["JiraSettings:ProjectKey"] ?? "KAN";
    }

    private HttpClient CreateClient()
    {
        if (string.IsNullOrEmpty(_session.Email) || string.IsNullOrEmpty(_session.ApiToken) || string.IsNullOrEmpty(_session.BaseUrl))
        {
            throw new InvalidOperationException("Jira session is incomplete");
        }

        var client = _factory.CreateClient();
        var decryptedToken = _authService.UnprotectToken(_session.ApiToken);
        var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_session.Email}:{decryptedToken}"));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.BaseAddress = new Uri(_session.BaseUrl);

        return client;
    }

    public async Task<string?> FindUserAccountIdAsync(string name)
    {
        using var client = CreateClient();
        var response = await client.GetAsync($"/rest/api/3/user/search?query={Uri.EscapeDataString(name)}");
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<List<JiraUser>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return users?.FirstOrDefault()?.accountId;
    }

    public async Task<List<JiraUser>> GetAllUsersAsync()
    {
        using var client = CreateClient();
        var response = await client.GetAsync("/rest/api/3/users/search?query=.&maxResults=1000");
        if (!response.IsSuccessStatusCode) return new();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<JiraUser>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
    }

    public async Task<List<JiraProject>> GetProjectsAsync()
    {
        using var client = CreateClient();
        var response = await client.GetAsync("/rest/api/3/project");

        if (!response.IsSuccessStatusCode) return new();

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<JiraProject>>(json, _jsonOptions) ?? new();
    }

    public record JiraProject(string id, string key, string name);
    public async Task<bool> CreateIssueAsync(string projectKey, string summary, string description,string title, DateTime? startDate, DateTime? endDate, string priorityName, string? assigneeId = null)
    {
        using var client = CreateClient();
        var payload = new
        {
            fields = new
            {
                project = new { key = projectKey },
                summary=title,
                priority = new { name = priorityName },
                issuetype = new { name = "Task" },
                duedate = endDate?.ToString("2026-MM-dd"),
                customfield_10015 = startDate?.ToString("yyyy-MM-dd"),
                assignee = assigneeId != null ? new { accountId = assigneeId } : null,
                description = new
                {
                    type = "doc",
                    version = 1,
                    content = new[]
                    {
                        new { type = "paragraph", content = new[] { new { type = "text", text = description } } }
                    }
                }
            }
        };

        var response = await client.PostAsJsonAsync("/rest/api/3/issue", payload);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Jira API Error: {response.StatusCode} - {errorContent}");
        }

        return response.IsSuccessStatusCode;
    }

    public class JiraUser
    {
        public string accountId { get; set; } = "";
        public string displayName { get; set; } = "";
    }
}