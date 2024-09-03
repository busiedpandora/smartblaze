using Microsoft.AspNetCore.Identity;
using SmartBlaze.Backend.Dtos;
using SmartBlaze.Backend.Repositories;
using SmartBlaze.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<IChatSessionRepository, ChatSessionRepository>();
builder.Services.AddSingleton<IMessageRepository, MessageRepository>();
builder.Services.AddSingleton<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddSingleton<IUserRepository, UserRepository>();

builder.Services.AddSingleton<IChatSessionService, ChatSessionService>();
builder.Services.AddSingleton<IMessageService, MessageService>();
builder.Services.AddSingleton<IChatbotService, ChatbotService>();
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
builder.Services.AddSingleton<IUserService, UserService>();

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
