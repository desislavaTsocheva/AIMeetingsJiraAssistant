using Atlassian.Jira;
using JiraAiTool.Models;

public class JiraService
{
    private readonly Jira _jira;
    private readonly string _projectKey = "KAN";

    public JiraService(IConfiguration config)
    {
        var url = config["JiraSettings:BaseUrl"];
        var email = config["JiraSettings:Email"];
        var token = config["JiraSettings:ApiToken"];

        _jira = Jira.CreateRestClient(url, email, token);
    }

    public async Task<string> CreateIssueAsync(TaskItem task)
    {
        try
        {
            if (_jira == null) throw new Exception("Jira Client is not initialized!");

            var issue = _jira.CreateIssue(_projectKey);
            issue.Summary = task.Description;
            issue.Type = "Task";

            var createdIssue = await issue.SaveChangesAsync();
            return createdIssue.Key.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"JIRA API ERROR: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"INNER ERROR: {ex.InnerException.Message}");
            return null;
        }
    }
}