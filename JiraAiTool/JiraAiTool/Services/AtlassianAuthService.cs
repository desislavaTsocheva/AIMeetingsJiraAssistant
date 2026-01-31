using JiraAiTool.Models;
using Microsoft.AspNetCore.DataProtection;
using System.Net.Http.Json;

namespace JiraAiTool.Services;

public class AtlassianAuthService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly IDataProtector _protector;

    public AtlassianAuthService(HttpClient http, IConfiguration config, IDataProtectionProvider provider)
    {
        _http = http;
        _config = config;
        _protector = provider.CreateProtector("Jira.ApiToken.v1");
    }

    public async Task<JiraTokenResponse> ExchangeCodeAsync(string code)
    {
        var payload = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = _config["Atlassian:ClientId"]!,
            ["client_secret"] = _config["Atlassian:ClientSecret"]!,
            ["code"] = code,
            ["redirect_uri"] = _config["Atlassian:RedirectUri"]!
        };

        var response = await _http.PostAsync("https://auth.atlassian.com/oauth/token", new FormUrlEncodedContent(payload));

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception(body);
        }

        return await response.Content.ReadFromJsonAsync<JiraTokenResponse>() ?? throw new Exception("Token parse failed");
    }

    public string ProtectToken(string rawToken)
    {
        if (string.IsNullOrEmpty(rawToken)) return string.Empty;
        return _protector.Protect(rawToken);
    }

    public string UnprotectToken(string encryptedToken)
    {
        if (string.IsNullOrEmpty(encryptedToken)) return string.Empty;
        try
        {
            return _protector.Unprotect(encryptedToken);
        }
        catch
        {
            return "DECRYPTION_FAILED";
        }
    }
}