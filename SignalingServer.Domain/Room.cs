namespace SignalingServer.Domain
{
    public class Room
    {
        public string RoomId { get; private set; }
        public HashSet<Participant> Participants { get; private set; }
        
        public Room(string roomId)
        {
            RoomId = roomId;
            Participants = new HashSet<Participant>();
        }
        
        public void AddParticipant(Participant participant)
        {
            Participants.Add(participant);
        }
        
        public void RemoveParticipant(string connectionId)
        {
            var participant = Participants.FirstOrDefault(p => p.ConnectionId == connectionId);
            if (participant != null)
                Participants.Remove(participant);
        }
    }
}
