using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using SignalingServer.Core.Services;
using SignalingServer.Domain.Interfaces;

namespace SignalingServer.Tests.Infrastructure;

public abstract class TestBase : IAsyncDisposable
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly IRoomService RoomService;
    
    protected TestBase()
    {
        var services = new ServiceCollection();
        services.AddSignalR();
        services.AddSingleton<IRoomService, RoomService>(); // 注册 RoomService 作为 IRoomService 的实现
        
        ServiceProvider = services.BuildServiceProvider();
        RoomService = ServiceProvider.GetRequiredService<IRoomService>();
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (ServiceProvider is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
    }
}