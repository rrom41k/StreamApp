﻿FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["StreamAppApi.App/StreamAppApi.App.csproj", "StreamAppApi.App/"]
COPY ["StreamAppApi.Bll/StreamAppApi.Bll.csproj", "StreamAppApi.Bll/"]
COPY ["StreamAppApi.Contracts/StreamAppApi.Contracts.csproj", "StreamAppApi.Contracts/"]
RUN dotnet restore "StreamAppApi.App/StreamAppApi.App.csproj"
COPY . .
WORKDIR "/src/StreamAppApi.App"
RUN dotnet build "StreamAppApi.App.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "StreamAppApi.App.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StreamAppApi.App.dll"]
