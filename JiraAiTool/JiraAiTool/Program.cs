using JiraAiTool.Components;
using JiraAiTool.Models;
using JiraAiTool.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=meetings.db", o => o.CommandTimeout(30)));

builder.Services.AddScoped<OllamaService>();
builder.Services.AddScoped<JiraService>();
builder.Services.AddScoped<FileProcessingService>();
builder.Services.AddScoped<UserSession>();

builder.Services.AddDataProtection();

builder.Services.AddHttpClient<AtlassianAuthService>();

builder.Services.AddHttpClient("Atlassian", client =>
{
    client.BaseAddress = new Uri("https://auth.atlassian.com/");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();
