﻿# Build runtime image
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as builder
WORKDIR /app
COPY . .
RUN dotnet restore "Server/ProtoFiles/ProtoFiles.csproj"
RUN dotnet build "Server/ProtoFiles/ProtoFiles.csproj"
RUN dotnet restore "Core/SubterfugeCore/SubterfugeCore.csproj"
RUN dotnet build "Core/SubterfugeCore/SubterfugeCore.csproj"

FROM builder as test
WORKDIR /app
CMD ["dotnet", "test", "Server/SubterfugeServerTest/SubterfugeServerTest.csproj"]

FROM builder as publish
RUN dotnet publish "Server/SubterfugeServer/SubterfugeServer.csproj" -o out -f netcoreapp2.0 -r linux-x64 --self-contained true -c Release

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
COPY --from=publish /app/out/ .
EXPOSE 5000
ENTRYPOINT ["dotnet", "SubterfugeServer.dll"]