FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY . .
COPY ["BundestagMine/BundestagMine.csproj", "BundestagMine/"]
COPY ["BundestagMine.Models/BundestagMine.Models.csproj", "BundestagMine.Models/"]
COPY ["BundestagMine.MongoDB/BundestagMine.MongoDB.csproj", "BundestagMine.MongoDB/"]
COPY ["BundestagMine.SqlDatabase/BundestagMine.SqlDatabase.csproj", "BundestagMine.SqlDatabase/"]
COPY ["BundestagMine.Synchronisation/BundestagMine.Synchronisation.csproj", "BundestagMine.Synchronisation/"]
COPY ["BundestagMine.Utility/BundestagMine.Utility.csproj", "BundestagMine.Utility/"]

RUN dotnet restore "BundestagMine/BundestagMine.csproj"
COPY . .

WORKDIR "/src/BundestagMine"
RUN dotnet build "BundestagMine.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "BundestagMine.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BundestagMine.dll"]