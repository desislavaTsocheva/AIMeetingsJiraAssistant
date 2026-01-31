using JiraAiTool.Models;
using System.Net.Http.Json;
using static JiraAiTool.Components.Pages.Callback;

namespace JiraAiTool.Services;

public class AtlassianAuthService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public AtlassianAuthService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<Models.JiraTokenResponse> ExchangeCodeAsync(string code)
    {
        var payload = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = _config["Jira:ClientId"]!,
            ["client_secret"] = _config["Jira:ClientSecret"]!,
            ["code"] = code,
            ["redirect_uri"] = _config["Jira:RedirectUri"]!
        };

        var response = await _http.PostAsync(
            "https://auth.atlassian.com/oauth/token",
            new FormUrlEncodedContent(payload)
        );

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception(body);
        }

        return await response.Content.ReadFromJsonAsync<Models.JiraTokenResponse>()
               ?? throw new Exception("Token parse failed");
    }
}
