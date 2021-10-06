# Build runtime image
FROM mcr.microsoft.com/dotnet/sdk:5.0 as builder
WORKDIR /app
COPY . .
RUN dotnet build "Server/ProtoFiles/ProtoFiles.csproj"
RUN dotnet build "Core/SubterfugeCore/SubterfugeCore.csproj"

FROM builder as test
WORKDIR /app
CMD ["dotnet", "test", "Server/SubterfugeServerTest/SubterfugeServerTest.csproj"]

FROM builder as publish
RUN dotnet publish "Server/SubterfugeServer/SubterfugeServer.csproj" -o out -c Release

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:5.0
RUN apt-get update && apt-get install libunwind8 libnotify4 libssl1.1 -y
WORKDIR /app
COPY --from=publish /app/out/ .
EXPOSE 5000
ENTRYPOINT ["dotnet", "SubterfugeServer.dll"]