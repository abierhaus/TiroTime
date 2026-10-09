# TiroTime - Docker Deployment

Diese Anleitung beschreibt, wie Sie TiroTime in einem Docker-Container deployen.

## Voraussetzungen

- Docker Desktop für Windows installiert
- SQL Server auf localhost läuft
- Datenbank "TiroTime" ist angelegt
- SQL Server User "usrTiroTime" hat Zugriff auf die Datenbank

## Schnellstart

### 1. Container bauen und starten

```bash
docker compose up -d --build
```

Der Container startet automatisch und ist unter http://localhost:5000 erreichbar.

### 2. Logs anzeigen

```bash
docker compose logs -f
```

### 3. Container stoppen

```bash
docker compose down
```

## Wichtige Hinweise

### Automatischer Start mit Docker

Der Container ist mit `restart: always` konfiguriert und startet automatisch:
- Beim Systemstart (wenn Docker läuft)
- Nach einem Neustart des Containers
- Nach einem Fehler

### Verbindung zum Host-SQL-Server

Der Container verwendet `host.docker.internal` um auf den SQL Server auf dem Host zuzugreifen:

```
Server=host.docker.internal;Database=TiroTime;User Id=usrTiroTime;Password=P@ssw0rd
```

**Windows:** `host.docker.internal` funktioniert automatisch
**Linux:** Verwenden Sie stattdessen die IP-Adresse des Docker-Host-Netzwerks (z.B. `172.17.0.1`)

### Ports

- **HTTP:** http://localhost:5000 (Host-Port 5000 -> Container-Port 8080)
- **Health-Check:** http://localhost:5000/health

## Konfiguration anpassen

### Environment-Variablen

Erstellen Sie eine `.env` Datei im Root-Verzeichnis (basierend auf `.env.example`):

```bash
cp .env.example .env
```

Passen Sie die Werte in der `.env` Datei an:

```env
ConnectionStrings__DefaultConnection=Server=host.docker.internal;Database=TiroTime;User Id=usrTiroTime;Password=IhrPasswort;TrustServerCertificate=True
SeedUsers__StandardUser__Email=benutzer@tirotime.com
SeedUsers__StandardUser__Password=IhrSicheresPasswort
```

Die `.env` Datei wird automatisch von Docker Compose geladen und ist in `.gitignore` ausgeschlossen.

### docker-compose.yml anpassen

Bearbeiten Sie `docker-compose.yml` um:
- Ports zu ändern
- Weitere Environment-Variablen hinzuzufügen
- Volumes zu konfigurieren

## Datenbank-Migrationen

Beim ersten Start wendet die Anwendung automatisch alle Datenbank-Migrationen an und erstellt die initialen Benutzer:

- **Admin:** admin@tirotime.com (Passwort: Admin123!@#$)
- **Standard-User:** Aus Environment-Variablen

## Troubleshooting

### Container startet nicht

Logs prüfen:
```bash
docker compose logs tirotime-web
```

### Keine Verbindung zur Datenbank

1. Prüfen Sie, ob SQL Server läuft und von außen erreichbar ist
2. Prüfen Sie die Firewall-Einstellungen für Port 1433
3. SQL Server muss TCP/IP-Verbindungen akzeptieren (SQL Server Configuration Manager)
4. Bei Linux: Verwenden Sie die Docker-Host-IP statt `host.docker.internal`

Testen Sie die Verbindung vom Container:
```bash
docker exec -it tirotime-app bash
```

### Container neu bauen

Falls Sie Änderungen am Code vorgenommen haben:

```bash
docker compose down
docker compose build --no-cache
docker compose up -d
```

## Dockerfile Details

Das Dockerfile verwendet einen **Multi-Stage Build** für optimale Image-Größe:

### Build Stage
1. Kopiert `global.json`, `Directory.Build.props`, `Directory.Packages.props` und die .csproj-Dateien (Restore-Layer wird gecacht)
2. Führt `dotnet restore` nur für das Web-Projekt aus
3. Kopiert nur die `src/` Quelldateien (Tests bleiben außen vor)
4. Baut und publisht das Web-Projekt im Release-Modus

### Runtime Stage
1. Verwendet schlankes aspnet:10.0 Runtime-Image, läuft als unprivilegierter Benutzer (`USER $APP_UID`) auf Port 8080; publiziert wird nur für `linux-x64` (keine fremden nativen Bibliotheken im Image)
2. Kopiert nur die kompilierten Binaries und bringt einen `HEALTHCHECK` auf `/health` mit
3. Enthält keine Build-Tools oder Test-Projekte

**Ergebnis:** Schlankes Produktions-Image ohne unnötige Dependencies.

## Produktion

Für die Produktion:

1. Ändern Sie alle Standard-Passwörter
2. Verwenden Sie sichere JWT-SecretKeys (mindestens 32 Zeichen)
3. Konfigurieren Sie HTTPS mit einem gültigen Zertifikat
4. Verwenden Sie Azure Key Vault oder ähnliche Dienste für Secrets
5. Setzen Sie `ASPNETCORE_ENVIRONMENT=Production`

## Nützliche Befehle

```bash
# Container Status anzeigen
docker compose ps

# Container neu starten
docker compose restart

# In den Container einloggen
docker exec -it tirotime-app bash

# Container-Ressourcen anzeigen
docker stats tirotime-app

# Alle Container und Images aufräumen
docker system prune -a
```

## Health-Check

Der Container meldet seinen Zustand über `GET /health` (prüft auch die Datenbankverbindung).
Docker wertet den Endpunkt automatisch per `HEALTHCHECK` aus:

```bash
docker inspect --format '{{.State.Health.Status}}' tirotime-app
```

## Data-Protection-Keys

Die Schlüssel für die Cookie-Verschlüsselung liegen im benannten Volume `tirotime-dataprotection`
(`/home/app/.aspnet/DataProtection-Keys`). Dadurch bleiben Anmeldungen auch nach `docker compose up -d`
mit einem neuen Image gültig. Ohne das Volume würde jeder neue Container neue Schlüssel erzeugen.
