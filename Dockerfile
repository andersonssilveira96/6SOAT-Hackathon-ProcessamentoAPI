FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080
RUN apk add --no-cache ffmpeg

USER app
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
RUN apk add --upgrade --no-cache ca-certificates && update-ca-certificates
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY src/FIAPX.Processamento.Api/FIAPX.Processamento.sln /
COPY src ./src

# Restoring just the api project will cause a chain reaction that will resolve the nuget dependencies from all class libraries.
RUN dotnet restore src/FIAPX.Processamento.Api/FIAPX.Processamento.Api.csproj

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/FIAPX.Processamento.Api/FIAPX.Processamento.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FIAPX.Processamento.Api.dll"]