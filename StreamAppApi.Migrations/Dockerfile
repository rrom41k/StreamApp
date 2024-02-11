FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["StreamAppApi.Migrations/StreamAppApi.Migrations.csproj", "StreamAppApi.Migrations/"]
COPY ["StreamAppApi.Bll/StreamAppApi.Bll.csproj", "StreamAppApi.Bll/"]
COPY ["StreamAppApi.Contracts/StreamAppApi.Contracts.csproj", "StreamAppApi.Contracts/"]
RUN dotnet restore "StreamAppApi.Migrations/StreamAppApi.Migrations.csproj"
COPY . .
WORKDIR "/src/StreamAppApi.Migrations"
RUN dotnet build "StreamAppApi.Migrations.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "StreamAppApi.Migrations.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StreamAppApi.Migrations.dll"]
