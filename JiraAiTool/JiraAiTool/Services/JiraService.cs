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

    public async Task<string?> FindUserAccountIdAsync(string name)
    {
        var response = await _http.GetAsync(
            $"{_baseUrl}/rest/api/3/user/search?query={Uri.EscapeDataString(name)}"
        );

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return null;

        var users = JsonSerializer.Deserialize<List<JiraUser>>(json);

        return users?.FirstOrDefault()?.accountId;
    }

    public async Task<List<JiraUser>> GetAllUsersAsync()
    {
        var url = $"{_baseUrl}/rest/api/3/users/search?query=.&maxResults=1000";

        var request = new HttpRequestMessage(HttpMethod.Get, url);

        var response = await _http.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        Console.WriteLine("Jira users response:");
        Console.WriteLine(json);

        if (!response.IsSuccessStatusCode)
            return new List<JiraUser>();

        return JsonSerializer.Deserialize<List<JiraUser>>(json) ?? new();
    }

    public async Task<bool> CreateIssueAsync(
     string summary,
     string description,
     DateTime? startDate,
     DateTime? endDate,
     string? assigneeId = null)
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

                issuetype = new { name = "Task" },
                duedate = endDate?.ToString("yyyy-MM-dd"),
                customfield_10015 = startDate?.ToString("yyyy-MM-dd"),
                assignee = assigneeId != null ? new { id = assigneeId } : null
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync($"{_baseUrl}/rest/api/3/issue", content);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine("JIRA ERROR:");
            Console.WriteLine(body);
            return false;
        }

        Console.WriteLine("Jira task created with Start + Due date");
        return true;
    }

    public class JiraUser
    {
        public string accountId { get; set; } = "";
        public string displayName { get; set; } = "";
        public string emailAddress { get; set; } = "";
    }

}
