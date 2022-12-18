FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Server/SubterfugeRestApiServerTest/SubterfugeRestApiServerTest.csproj", "SubterfugeRestApiServerTest/"]
RUN dotnet restore "Server/SubterfugeRestApiServerTest/SubterfugeRestApiServerTest.csproj"
COPY . .
WORKDIR "/src/SubterfugeRestApiServerTest"
RUN dotnet build "SubterfugeRestApiServerTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SubterfugeRestApiServerTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SubterfugeRestApiServerTest.dll"]
