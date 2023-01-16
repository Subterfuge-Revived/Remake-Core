FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY ./Server/SubterfugeRestApiServer/SubterfugeRestApiServer.csproj ./Server/SubterfugeRestApiServer/SubterfugeRestApiServer.csproj
RUN dotnet restore ./Server/SubterfugeRestApiServer/SubterfugeRestApiServer.csproj
COPY . .
RUN dotnet build ./Server/SubterfugeRestApiServer/SubterfugeRestApiServer.csproj -c Release -o /app/build 

FROM build as test
WORKDIR /app
CMD ["dotnet", "test", "./Server/SubterfugeRestApiServerTest/SubterfugeRestApiServerTest.csproj"]

FROM build AS publish
RUN dotnet publish ./Server/SubterfugeRestApiServer/SubterfugeRestApiServer.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ./Server/SubterfugeRestApiServer/appsettings.Docker.json ./appsettings.Docker.json
ENTRYPOINT ["dotnet", "SubterfugeRestApiServer.dll", "--launch-profile", "Docker"]