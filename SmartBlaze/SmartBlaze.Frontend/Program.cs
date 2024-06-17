using System.Collections.Immutable;
using Microsoft.AspNetCore.Components;
using SmartBlaze.Backend.Controllers;
using SmartBlaze.Backend.Services;
using SmartBlaze.Frontend.Components;
using SmartBlaze.Frontend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<ChatSessionService>();
builder.Services.AddSingleton<MessageService>();
builder.Services.AddSingleton<ChatSessionController>();

builder.Services.AddSingleton<ChatStateService>();

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