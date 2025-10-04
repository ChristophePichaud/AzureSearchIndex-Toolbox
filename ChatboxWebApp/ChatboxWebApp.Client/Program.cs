using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http;
using ChatboxWebApp.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register the custom handler
//builder.Services.AddScoped<CustomAuthorizationMessageHandler>();

// Register HttpClient using the custom handler - ONLY ONE REGISTRATION
builder.Services.AddScoped<HttpClient>(sp =>
{
    var handler = sp.GetRequiredService<CustomAuthorizationMessageHandler>();
    return new HttpClient(handler)
    {
        BaseAddress = new Uri("http://localhost:5077/") // Adresse de l'API
    };
});

await builder.Build().RunAsync();

// Handler personnalisé pour ajouter le token JWT
public class CustomAuthorizationMessageHandler : DelegatingHandler
{
    private readonly IJSRuntime _js;

    public CustomAuthorizationMessageHandler(IJSRuntime js)
    {
        _js = js;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
