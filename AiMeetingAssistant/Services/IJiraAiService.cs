using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiMeetingAssistant.Services
{
    public interface IJiraAiService
    {
        Task<List<MeetingTask>> ProcessTranscriptionAsync(string text);
        Task<bool> SendToJiraAsync(List<MeetingTask> tasks);
    }

    public class JiraTask
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; } = "Medium";
    }
}
