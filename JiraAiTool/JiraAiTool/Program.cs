using JiraAiTool.Components;
using JiraAiTool.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        "Data Source=meetings.db;Cache=Shared;Pooling=False",
        o => o.CommandTimeout(60)
    ));


builder.Services.AddScoped<OllamaService>();
builder.Services.AddScoped<JiraService>();
builder.Services.AddScoped<FileProcessingService>();

builder.Services.AddHttpClient("Atlassian", client =>
{
    client.BaseAddress = new Uri("https://auth.atlassian.com/");
});

builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
    });


builder.Services.AddServerSideBlazor().AddHubOptions(options =>
{
    options.MaximumReceiveMessageSize = 32 * 1024 * 1024; 
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
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
