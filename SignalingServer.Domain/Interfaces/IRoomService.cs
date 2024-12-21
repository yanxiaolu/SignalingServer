namespace SignalingServer.Domain.Interfaces
{
    public interface IRoomService
    {
        Task<Room> CreateOrJoinRoom(string roomId, string userId, string connectionId);
        Task LeaveRoom(string roomId, string connectionId);
        Task<IEnumerable<Participant>> GetParticipants(string roomId);
    }
}