using SignalingServer.Domain;
using SignalingServer.Domain.Interfaces;

namespace SignalingServer.Core.Services
{
    public class RoomService : IRoomService
    {
        private readonly Dictionary<string, Room> _rooms = new();
        private readonly object _lock = new();

        public async Task<Room> CreateOrJoinRoom(string roomId, string userId, string connectionId)
        {
            lock (_lock)
            {
                if (!_rooms.ContainsKey(roomId))
                {
                    _rooms[roomId] = new Room(roomId);
                }

                var room = _rooms[roomId];
                var participant = new Participant(connectionId, userId);
                room.AddParticipant(participant);

                return room;
            }
        }

        public Task LeaveRoom(string roomId, string connectionId)
        {
            lock (_lock)
            {
                if (_rooms.TryGetValue(roomId, out var room))
                {
                    room.RemoveParticipant(connectionId);
                    if (room.Participants.Count == 0)
                    {
                        _rooms.Remove(roomId);
                    }
                }
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Participant>> GetParticipants(string roomId)
        {
            if (_rooms.TryGetValue(roomId, out var room))
            {
                return Task.FromResult(room.Participants.AsEnumerable());
            }
            return Task.FromResult(Enumerable.Empty<Participant>());
        }
    }
}