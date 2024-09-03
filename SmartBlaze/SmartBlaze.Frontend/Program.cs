using SmartBlaze.Frontend.Components;
using SmartBlaze.Frontend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var backendUrl = builder.Configuration["BackendUrlHTTPS"] ?? throw new Exception("BackendUrl is not set");

builder.Services.AddHttpClient("_httpClient", client =>
{
    client.BaseAddress = new Uri(backendUrl);
});

builder.Services.AddSingleton<ChatSessionStateService>();
builder.Services.AddSingleton<RedirectionService>();
builder.Services.AddSingleton<SettingsService>();
builder.Services.AddSingleton<UserStateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();