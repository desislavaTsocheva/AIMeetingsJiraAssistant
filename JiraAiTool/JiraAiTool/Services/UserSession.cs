namespace JiraAiTool.Services
{
    public class UserSession
    {
        public string Email { get; set; }
        public string ApiToken { get; set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(ApiToken);
    }
}
