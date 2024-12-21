using SignalingServer.Domain.Interfaces;
using SignalingServer.Core.Services;
using SignalingServer.Infrastructure.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

// Add services
builder.Services.AddSingleton<IRoomService, RoomService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map SignalR hub
app.MapHub<SignalingHub>("/signaling");

app.Run();