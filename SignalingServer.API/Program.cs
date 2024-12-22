using SignalingServer.Domain.Interfaces;
using SignalingServer.Core.Services;
using SignalingServer.Infrastructure.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", builder =>
    {
        builder
            .WithOrigins("http://localhost:5278") // Web Client的地址
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); // 允许凭据，SignalR需要这个
    });
});

// Add services
builder.Services.AddSingleton<IRoomService, RoomService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enable CORS before routing
app.UseCors("SignalRPolicy");

app.UseHttpsRedirection();

// Map SignalR hub
app.MapHub<SignalingHub>("/signaling");

app.Run();