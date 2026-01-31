namespace JiraAiTool.Models;

public class User
{
    public int Id { get; set; }
    public string JiraAccountId { get; set; } = default!;
    public string? ApiToken { get; set; } = default!;
    public string? BaseUrl { get; set; }
    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; }
}
