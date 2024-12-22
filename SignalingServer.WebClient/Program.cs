using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SignalingServer.WebClient;
using SignalingServer.WebClient.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 注册服务
builder.Services.AddScoped<SignalingService>();
builder.Services.AddScoped<WebRTCService>();

// 配置HttpClient
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.Configuration.GetSection("ApiServer:BaseUrl").Value!) 
});

await builder.Build().RunAsync();
