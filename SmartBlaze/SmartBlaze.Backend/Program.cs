using Microsoft.AspNetCore.Identity;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Repositories;
using SmartBlaze.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<ChatSessionRepository>();
builder.Services.AddSingleton<MessageRepository>();
builder.Services.AddSingleton<ConfigurationRepository>();
builder.Services.AddSingleton<UserRepository>();

builder.Services.AddSingleton<ChatSessionService>();
builder.Services.AddSingleton<MessageService>();
builder.Services.AddSingleton<ChatbotService>();
builder.Services.AddSingleton<ConfigurationService>();
builder.Services.AddSingleton<UserService>();

builder.Services.AddSingleton<IPasswordHasher<UserDto>, PasswordHasher<UserDto>>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
