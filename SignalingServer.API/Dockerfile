FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SignalingServer.API/SignalingServer.API.csproj", "SignalingServer.API/"]
COPY ["SignalingServer.Core/SignalingServer.Core.csproj", "SignalingServer.Core/"]
COPY ["SignalingServer.Domain/SignalingServer.Domain.csproj", "SignalingServer.Domain/"]
COPY ["SignalingServer.Infrastructure/SignalingServer.Infrastructure.csproj", "SignalingServer.Infrastructure/"]
RUN dotnet restore "SignalingServer.API/SignalingServer.API.csproj"
COPY . .
WORKDIR "/src/SignalingServer.API"
RUN dotnet build "SignalingServer.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SignalingServer.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SignalingServer.API.dll"]
