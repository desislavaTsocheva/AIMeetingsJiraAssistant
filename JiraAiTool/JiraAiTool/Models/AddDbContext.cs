using JiraAiTool.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<MeetingDocument> Documents { get; set; }
    public DbSet<User> Users { get; set; } 
    public DbSet<TaskResource> TaskResources { get; set; } 
}