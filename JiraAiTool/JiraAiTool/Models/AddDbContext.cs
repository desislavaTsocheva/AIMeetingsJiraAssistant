using JiraAiTool.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        Database.OpenConnection();
        using var command = Database.GetDbConnection().CreateCommand();
        command.CommandText = "PRAGMA journal_mode=WAL;";
        command.ExecuteNonQuery();
    }

    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<MeetingDocument> Documents { get; set; }
}