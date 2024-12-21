namespace SignalingServer.Domain.Interfaces
{
    public interface ISignalingHub
    {
        Task JoinRoom(string roomId);
        Task LeaveRoom(string roomId);
        Task SendOffer(string roomId, string targetUserId, string offer);
        Task SendAnswer(string roomId, string targetUserId, string answer);
        Task SendIceCandidate(string roomId, string targetUserId, string candidate);
    }
}