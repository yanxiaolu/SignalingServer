namespace SignalingServer.Domain
{
    public class Participant
    {
        public string ConnectionId { get; private set; }
        public string UserId { get; private set; }
        
        public Participant(string connectionId, string userId)
        {
            ConnectionId = connectionId;
            UserId = userId;
        }
    }
}
