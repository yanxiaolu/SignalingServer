using SignalingServer.Core.Services;
using SignalingServer.Domain;
using SignalingServer.Tests.Infrastructure;
using Xunit;

namespace SignalingServer.Tests.Services;

public class RoomServiceTests : TestBase
{
    private readonly RoomService _roomService;

    public RoomServiceTests()
    {
        _roomService = new RoomService();
    }

    [Fact]
    public async Task CreateOrJoinRoom_ShouldCreateNewRoom_WhenRoomDoesNotExist()
    {
        // Arrange
        var roomId = "testRoom";
        var userId = "user1";
        var connectionId = "connection1";

        // Act
        var room = await _roomService.CreateOrJoinRoom(roomId, userId, connectionId);

        // Assert
        Assert.NotNull(room);
        Assert.Equal(roomId, room.RoomId);
        Assert.Single(room.Participants);
    }

    [Fact]
    public async Task CreateOrJoinRoom_ShouldJoinExistingRoom_WhenRoomExists()
    {
        // Arrange
        var roomId = "testRoom";
        var userId1 = "user1";
        var userId2 = "user2";
        var connectionId1 = "connection1";
        var connectionId2 = "connection2";

        // Act
        var room1 = await _roomService.CreateOrJoinRoom(roomId, userId1, connectionId1);
        var room2 = await _roomService.CreateOrJoinRoom(roomId, userId2, connectionId2);

        // Assert
        Assert.NotNull(room2);
        Assert.Equal(roomId, room2.RoomId);
        Assert.Equal(2, room2.Participants.Count);
    }
}