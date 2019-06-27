FROM mcr.microsoft.com/dotnet/core/runtime:2.1-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["Lighthouse.Server/Lighthouse.Server.csproj", "Lighthouse.Server/"]
COPY ["Lighthouse.Storage/Lighthouse.Storage.csproj", "Lighthouse.Storage/"]
COPY ["Lighthouse.Core/Lighthouse.Core.csproj", "Lighthouse.Core/"]
RUN dotnet restore "Lighthouse.Server/Lighthouse.Server.csproj"
COPY . .
WORKDIR "/src/Lighthouse.Server"
RUN dotnet build "Lighthouse.Server.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Lighthouse.Server.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Lighthouse.Server.dll", "run", "-app", "ping"]