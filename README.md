# SignalingServer - WebRTC信令服务器

基于.NET 9.0的WebRTC信令服务器，采用Clean Architecture架构设计，提供可靠的实时通信服务。

## 项目简介

SignalingServer是一个用于WebRTC应用的信令服务器，负责管理用户连接、房间管理以及信令消息的传递。该项目采用最新的.NET技术栈，提供高性能、可扩展的实时通信解决方案。

## 技术架构

项目采用Clean Architecture架构，分为以下几层：

```
SignalingServer/
├── SignalingServer.API            # API层：处理HTTP和WebSocket请求
├── SignalingServer.Core           # 核心业务层：实现业务逻辑
├── SignalingServer.Domain         # 领域层：定义领域模型和接口
├── SignalingServer.Infrastructure # 基础设施层：实现具体功能
└── SignalingServer.Tests          # 测试项目
```

### 核心功能

- 房间管理：创建、加入、离开房间
- 信令传递：处理Offer/Answer交换
- ICE候选者处理：管理连接协商
- 实时通信：基于SignalR的实时消息传递

## API文档

### SignalR Hub方法

| 方法 | 参数 | 说明 |
|------|------|------|
| JoinRoom | roomId: string | 加入指定房间 |
| LeaveRoom | roomId: string | 离开指定房间 |
| SendOffer | roomId: string, targetUserId: string, offer: string | 发送WebRTC offer |
| SendAnswer | roomId: string, targetUserId: string, answer: string | 发送WebRTC answer |
| SendIceCandidate | roomId: string, targetUserId: string, candidate: string | 发送ICE候选者 |

### 客户端事件

| 事件名 | 参数 | 说明 |
|--------|------|------|
| UserJoined | connectionId: string | 用户加入通知 |
| UserLeft | connectionId: string | 用户离开通知 |
| ReceiveOffer | fromUserId: string, offer: string | 接收Offer |
| ReceiveAnswer | fromUserId: string, answer: string | 接收Answer |
| ReceiveIceCandidate | fromUserId: string, candidate: string | 接收ICE候选者 |

## 部署指南

### Docker部署

```bash
# 构建Docker镜像
docker build -t signaling-server .

# 运行容器
docker run -d -p 8080:8080 -p 8081:8081 signaling-server
```

### 本地开发

1. 确保安装.NET 9.0 SDK
2. 克隆项目
3. 运行以下命令：

```bash
dotnet restore
dotnet build
dotnet run --project SignalingServer.API
```

## 开发指南

### 环境要求

- .NET 9.0 SDK
- Visual Studio 2022或VS Code
- Docker（可选，用于容器化部署）

### 调试

1. VS Code：
   - 打开项目文件夹
   - 按F5开始调试

2. Visual Studio 2022：
   - 打开解决方案文件
   - 选择SignalingServer.API项目
   - 点击开始调试

## 项目特点

- 采用Clean Architecture架构设计
- 遵循SOLID设计原则
- 完整的单元测试覆盖
- Docker容器化支持
- 基于SignalR的实时通信
- 支持WebRTC信令处理

## 贡献指南

1. Fork本项目
2. 创建特性分支
3. 提交更改
4. 发起Pull Request

## 许可证

本项目采用MIT许可证。详见[LICENSE](LICENSE)文件。