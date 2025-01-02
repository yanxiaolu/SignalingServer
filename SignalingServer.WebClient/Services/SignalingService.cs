using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace SignalingServer.WebClient.Services;

public class SignalingService : IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private readonly ILogger<SignalingService> _logger;

    public event Action<string>? OnUserJoined;
    public event Action<string>? OnUserLeft;
    public event Action<string, string>? OnReceiveOffer;
    public event Action<string, string>? OnReceiveAnswer;
    public event Action<string, string>? OnReceiveIceCandidate;
    public event Action<string>? OnSendOfferToNewUser;


    public SignalingService(IConfiguration configuration, ILogger<SignalingService> logger)
    {
        _logger = logger;
        var baseUrl = configuration.GetSection("ApiServer:BaseUrl").Value;
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/signaling")  // 使用完整的API服务器地址
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string>("UserJoined", (connectionId) => OnUserJoined?.Invoke(connectionId));
        _hubConnection.On<string>("UserLeft", (connectionId) => OnUserLeft?.Invoke(connectionId));
        _hubConnection.On<string, string>("ReceiveOffer", (fromUserId, offer) => OnReceiveOffer?.Invoke(fromUserId, offer));
        _hubConnection.On<string, string>("ReceiveAnswer", (fromUserId, answer) => OnReceiveAnswer?.Invoke(fromUserId, answer));
        _hubConnection.On<string, string>("ReceiveIceCandidate", (fromUserId, candidate) => OnReceiveIceCandidate?.Invoke(fromUserId, candidate));
        _hubConnection.On<string>("SendOfferToNewUser", (newUserId) => OnSendOfferToNewUser?.Invoke(newUserId));

    }

    public async Task StartAsync()
    {
        await _hubConnection.StartAsync();
    }

    public async Task JoinRoomAsync(string roomId)
    {
        await _hubConnection.InvokeAsync("JoinRoom", roomId);
    }

    public async Task LeaveRoomAsync(string roomId)
    {
        await _hubConnection.InvokeAsync("LeaveRoom", roomId);
    }

    public async Task SendOfferAsync(string roomId, string targetUserId, string offer)
    {
        await _hubConnection.InvokeAsync("SendOffer", roomId, targetUserId, offer);
    }

    public async Task SendAnswerAsync(string roomId, string targetUserId, string answer)
    {
        await _hubConnection.InvokeAsync("SendAnswer", roomId, targetUserId, answer);
    }

    public async Task SendIceCandidateAsync(string roomId, string targetUserId, string candidate)
    {
        await _hubConnection.InvokeAsync("SendIceCandidate", roomId, targetUserId, candidate);
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
