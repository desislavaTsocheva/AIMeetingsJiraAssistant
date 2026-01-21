namespace JiraAiTool.Models
{
    public class MeetingDocument
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string RawContent { get; set; } = string.Empty; 
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public List<TaskItem> Tasks { get; set; } = new();
    }
}
