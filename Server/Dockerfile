# Build runtime image
FROM microsoft/aspnetcore-build:2.0 as builder
WORKDIR /app
COPY SubterfugeServer SubterfugeServer
COPY ProtoGenerated ProtoGenerated
RUN dotnet restore SubterfugeServer
RUN dotnet build SubterfugeServer
RUN dotnet publish SubterfugeServer -o out -f netcoreapp2.0 -r linux-x64 --self-contained true -c Release

# Build runtime image
FROM microsoft/aspnetcore-build:2.0
WORKDIR /app
COPY --from=builder /app/SubterfugeServer/out/ .
EXPOSE 5000
ENTRYPOINT ["dotnet", "SubterfugeServer.dll"]