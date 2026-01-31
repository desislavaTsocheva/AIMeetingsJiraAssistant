using Microsoft.EntityFrameworkCore;

namespace JiraAiTool.Services;

public class UserSession
{
    public string? Email { get; set; }
    public string? ApiToken { get; set; }
    public string? BaseUrl { get; set; }
    public string? JiraAccountId { get; set; }

    public bool IsReady => !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(ApiToken) && !string.IsNullOrEmpty(BaseUrl);

    public async Task<bool> LoadUserFromDb(AppDbContext db)
    {
        var user = await db.Users.OrderByDescending(u => u.CreatedAt).FirstOrDefaultAsync();
        if (user == null) return false;

        Email = user.Email;
        ApiToken = user.ApiToken;
        BaseUrl = user.BaseUrl;
        JiraAccountId = user.JiraAccountId;

        return true;
    }
}