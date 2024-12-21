using Microsoft.AspNetCore.SignalR;
using Moq;
using SignalingServer.Infrastructure.Hubs;
using SignalingServer.Tests.Infrastructure;
using Xunit;

namespace SignalingServer.Tests.Hubs;

public class SignalingHubTests : TestBase
{
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<HubCallerContext> _mockHubContext;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly SignalingHub _hub;

    public SignalingHubTests()
    {
        _mockClients = new Mock<IHubCallerClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockHubContext = new Mock<HubCallerContext>();
        _mockGroups = new Mock<IGroupManager>();

        _hub = new SignalingHub(RoomService)
        {
            Clients = _mockClients.Object,
            Context = _mockHubContext.Object,
            Groups = _mockGroups.Object
        };

        // 设置基本的 mock 行为
        _mockClients.Setup(c => c.All).Returns(_mockClientProxy.Object);
        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockHubContext.Setup(c => c.ConnectionId).Returns("testConnection");
    }

    [Fact]
    public async Task JoinRoom_ShouldNotifyOtherParticipants()
    {
        // Arrange
        var roomId = "testRoom";
        _mockClients.Setup(c => c.Group(roomId)).Returns(_mockClientProxy.Object);
        _mockGroups.Setup(x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .Returns(Task.CompletedTask);

        // Act
        await _hub.JoinRoom(roomId);

        // Assert
        _mockGroups.Verify(
            x => x.AddToGroupAsync("testConnection", roomId, default),
            Times.Once);
        
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "UserJoined",
                It.Is<object[]>(o => o[0].ToString() == "testConnection"),
                default),
            Times.Once);
    }
}