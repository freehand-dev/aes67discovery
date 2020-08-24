#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["AES67DiscoveryServer/AES67DiscoveryServer.csproj", "AES67DiscoveryServer/"]
RUN dotnet restore "AES67DiscoveryServer/AES67DiscoveryServer.csproj"
COPY . .
WORKDIR "/src/AES67DiscoveryServer"
RUN dotnet build "AES67DiscoveryServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AES67DiscoveryServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AES67DiscoveryServer.dll"]