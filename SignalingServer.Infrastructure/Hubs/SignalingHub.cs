using Microsoft.AspNetCore.SignalR;
using SignalingServer.Domain.Interfaces;

namespace SignalingServer.Infrastructure.Hubs
{
    public class SignalingHub : Hub, ISignalingHub
    {
        private readonly IRoomService _roomService;

        public SignalingHub(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public async Task JoinRoom(string roomId)
        {
            var room = await _roomService.CreateOrJoinRoom(roomId, Context.UserIdentifier, Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            
            // 通知房间内的其他用户(除了新用户)有新用户加入
            await Clients.OthersInGroup(roomId).SendAsync("UserJoined", Context.ConnectionId);
        }

        public async Task LeaveRoom(string roomId)
        {
            await _roomService.LeaveRoom(roomId, Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("UserLeft", Context.ConnectionId);
        }

        public async Task SendOffer(string roomId, string targetUserId, string offer)
        {
            await Clients.User(targetUserId).SendAsync("ReceiveOffer", Context.ConnectionId, offer);
        }

        public async Task SendAnswer(string roomId, string targetUserId, string answer)
        {
            await Clients.User(targetUserId).SendAsync("ReceiveAnswer", Context.ConnectionId, answer);
        }

        public async Task SendIceCandidate(string roomId, string targetUserId, string candidate)
        {
            await Clients.User(targetUserId).SendAsync("ReceiveIceCandidate", Context.ConnectionId, candidate);
        }
    }
}