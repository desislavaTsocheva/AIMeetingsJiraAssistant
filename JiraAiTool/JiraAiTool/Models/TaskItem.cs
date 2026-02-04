namespace JiraAiTool.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? AssigneeName { get; set; } 
        public string? JiraAccountId { get; set; } 
        public DateTime? Deadline { get; set; }
        public bool IsApproved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Priority { get; set; }
        public int MeetingDocumentId { get; set; }
        public MeetingDocument? Document { get; set; }
    }
}
