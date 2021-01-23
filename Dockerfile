# Build runtime image
FROM microsoft/aspnetcore-build:2.0 as builder
WORKDIR /app
COPY . .
RUN dotnet restore "Core/SubterfugeCore/SubterfugeCore.csproj"
RUN dotnet build "Core/SubterfugeCore/SubterfugeCore.csproj"
RUN dotnet publish "Server/SubterfugeServer/SubterfugeServer.csproj" -o out -f netcoreapp2.0 -r linux-x64 --self-contained true -c Release

# Build runtime image
FROM microsoft/aspnetcore-build:2.0
WORKDIR /app
COPY --from=builder /app/Server/SubterfugeServer/out/ .
EXPOSE 5000
ENTRYPOINT ["dotnet", "SubterfugeServer.dll"]