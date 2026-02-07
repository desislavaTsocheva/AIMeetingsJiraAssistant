namespace JiraAiTool.Models
{
    public class TaskResource
    {
        public int Id { get; set; }
        public string Type { get; set; } = "Link"; 
        public string Content { get; set; } = ""; 
        public string? LocalPath { get; set; } 
        public int TaskItemId { get; set; }
    }
}
