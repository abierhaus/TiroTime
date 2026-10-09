# syntax=docker/dockerfile:1

# ---------- Build Stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Build-Konfiguration (SDK-Version, gemeinsame Properties, zentrale Paketversionen)
COPY global.json Directory.Build.props Directory.Packages.props ./

# Projektdateien zuerst kopieren, damit der Restore-Layer gecacht wird
COPY src/TiroTime.Domain/TiroTime.Domain.csproj src/TiroTime.Domain/
COPY src/TiroTime.Application/TiroTime.Application.csproj src/TiroTime.Application/
COPY src/TiroTime.Infrastructure/TiroTime.Infrastructure.csproj src/TiroTime.Infrastructure/
COPY src/TiroTime.Web/TiroTime.Web.csproj src/TiroTime.Web/

# Nur für linux-x64 restoren/publizieren: native Bibliotheken (QuestPDF/SkiaSharp) anderer Plattformen bleiben draußen
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore src/TiroTime.Web/TiroTime.Web.csproj -r linux-x64

# Quellen kopieren (nur src/, keine Tests) und veröffentlichen
COPY src/ src/
WORKDIR /src/src/TiroTime.Web
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish --no-restore -p:UseAppHost=false

# ---------- Runtime Stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl nur für den Container-HEALTHCHECK; Verzeichnis für persistente Data-Protection-Keys (Cookie-Verschlüsselung)
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /home/app/.aspnet/DataProtection-Keys \
    && chown -R $APP_UID:$APP_UID /home/app/.aspnet

COPY --from=build /app/publish .

# Unprivilegierter Benutzer (im Basis-Image vorhanden); Port 8080 kommt aus ASPNETCORE_HTTP_PORTS des Basis-Images
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
USER $APP_UID

HEALTHCHECK --interval=30s --timeout=5s --start-period=60s --retries=3 \
    CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "TiroTime.Web.dll"]
