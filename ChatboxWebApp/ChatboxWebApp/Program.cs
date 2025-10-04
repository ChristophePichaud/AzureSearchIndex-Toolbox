using ChatboxWebApp.Client.Pages;
using ChatboxWebApp.Components;
using ChatboxWebApp.Models;
using ChatboxWebApp.Services;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// Add Controllers for API
builder.Services.AddControllers();

// Load ChatGPT configuration
var configPath = builder.Configuration["ChatGptConfigPath"] ?? "chatgpt-config.json";
ChatGptConfiguration? chatConfig = null;

if (File.Exists(configPath))
{
    var configJson = File.ReadAllText(configPath);
    chatConfig = JsonConvert.DeserializeObject<ChatGptConfiguration>(configJson);
}
else
{
    // Use configuration from appsettings.json
    chatConfig = builder.Configuration.GetSection("ChatGpt").Get<ChatGptConfiguration>();
}

if (chatConfig == null)
{
    throw new InvalidOperationException($"ChatGPT configuration not found. Please provide either '{configPath}' file or configuration in appsettings.json under 'ChatGpt' section.");
}

// Register ChatGptService as Singleton to maintain conversation state
builder.Services.AddSingleton(chatConfig);
builder.Services.AddSingleton<ChatGptService>();
builder.Services.AddScoped<HttpClient>();


// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseCors("AllowAll");
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// Map controllers
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(ChatboxWebApp.Client._Imports).Assembly);

app.Run();
