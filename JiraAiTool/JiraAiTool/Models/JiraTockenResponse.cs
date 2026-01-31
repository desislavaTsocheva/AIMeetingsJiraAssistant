namespace JiraAiTool.Models;

public class JiraTokenResponse
{
    public string access_token { get; set; } = default!;
    public string refresh_token { get; set; } = default!;
    public int expires_in { get; set; }
    public string token_type { get; set; } = default!;
}
