using Microsoft.JSInterop;

namespace SignalingServer.WebClient.Services;

public class WebRTCService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly SignalingService _signalingService;
    private readonly ILogger<WebRTCService> _logger;

    public WebRTCService(IJSRuntime jsRuntime, SignalingService signalingService, ILogger<WebRTCService> logger)
    {
        _jsRuntime = jsRuntime;
        _signalingService = signalingService;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        await _jsRuntime.InvokeVoidAsync("WebRTC.initialize");
    }

    public async Task<string> CreateOfferAsync(string targetUserId)
    {
        return await _jsRuntime.InvokeAsync<string>("WebRTC.createOffer", targetUserId);
    }

    public async Task<string> HandleOfferAsync(string fromUserId, string offer)
    {
        return await _jsRuntime.InvokeAsync<string>("WebRTC.handleOffer", fromUserId, offer);
    }

    public async Task HandleAnswerAsync(string fromUserId, string answer)
    {
        await _jsRuntime.InvokeVoidAsync("WebRTC.handleAnswer", fromUserId, answer);
    }

    public async Task HandleIceCandidateAsync(string fromUserId, string candidate)
    {
        await _jsRuntime.InvokeVoidAsync("WebRTC.handleIceCandidate", fromUserId, candidate);
    }

    public async Task DisposeAsync()
    {
        await _jsRuntime.InvokeVoidAsync("WebRTC.dispose");
    }
}