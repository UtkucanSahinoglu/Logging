FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Logging.Abstractions/Logging.Abstractions.csproj Logging.Abstractions/
COPY Logging.IngestionApi/Logging.IngestionApi.csproj Logging.IngestionApi/

RUN dotnet restore Logging.IngestionApi/Logging.IngestionApi.csproj

COPY . .

WORKDIR /src/Logging.IngestionApi

RUN dotnet build Logging.IngestionApi.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish Logging.IngestionApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Logging.IngestionApi.dll"]
