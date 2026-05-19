FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["VoxHubService.sln", "./"]
COPY ["VoxHubService/VoxHubService.csproj", "VoxHubService/"]

RUN dotnet restore "VoxHubService/VoxHubService.csproj"

COPY . .

WORKDIR /src/VoxHubService

RUN dotnet publish "VoxHubService.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "VoxHubService.dll"]