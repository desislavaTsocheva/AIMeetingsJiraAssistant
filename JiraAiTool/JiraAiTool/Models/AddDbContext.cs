using Microsoft.EntityFrameworkCore;

namespace JiraAiTool.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<MeetingDocument> Documents { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
    }
}
