# TiroTime Deployment Guide

## Secrets Management

Die Anwendung benötigt sensitive Konfigurationswerte (API Keys, Passwörter, Connection Strings), die **nicht** in Git eingecheckt werden dürfen.

## Entwicklungsumgebung (User Secrets)

Für die lokale Entwicklung werden User Secrets verwendet:

```bash
# Mailjet Konfiguration
dotnet user-secrets set "Mailjet:ApiKey" "YOUR_API_KEY" --project src/TiroTime.Web
dotnet user-secrets set "Mailjet:ApiSecret" "YOUR_API_SECRET" --project src/TiroTime.Web
dotnet user-secrets set "Mailjet:FromEmail" "your-email@example.com" --project src/TiroTime.Web
dotnet user-secrets set "Mailjet:FromName" "TiroTime" --project src/TiroTime.Web

# Alle Secrets anzeigen
dotnet user-secrets list --project src/TiroTime.Web
```

**Speicherort:**
- Windows: `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`
- Linux/macOS: `~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`

## Docker Container

### Option 1: Umgebungsvariablen (Empfohlen)

Docker unterstützt Umgebungsvariablen mit der Syntax `SectionName__SubSection__Key` (doppelte Unterstriche):

**docker-compose.yml:**
```yaml
version: '3.8'

services:
  tirotime-web:
    image: tirotime-web:latest
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=db;Database=TiroTime;User Id=usrTiroTime;Password=SecurePassword;TrustServerCertificate=True
      - Mailjet__ApiKey=${MAILJET_API_KEY}
      - Mailjet__ApiSecret=${MAILJET_API_SECRET}
      - Mailjet__FromEmail=${MAILJET_FROM_EMAIL}
      - Mailjet__FromName=TiroTime
      - Jwt__Secret=${JWT_SECRET}
    depends_on:
      - db

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong!Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

volumes:
  sqldata:
```

**.env Datei (nicht in Git!):**
```bash
MAILJET_API_KEY=
MAILJET_API_SECRET=
MAILJET_FROM_EMAIL=
JWT_SECRET=ThisIsAVerySecretKeyThatShouldBeAtLeast32CharactersLongForHS256Algorithm
```

**Starten:**
```bash
docker-compose --env-file .env up -d
```

### Option 2: Docker Secrets (für Docker Swarm)

**Secrets erstellen:**
```bash
echo "1e8e74e8110e090f0cd93cc55cf50ff6" | docker secret create mailjet_api_key -
echo "532f59e14eeec44a65efbcfe7944c4be" | docker secret create mailjet_api_secret -
echo "alex.bierhaus@abtree.de" | docker secret create mailjet_from_email -
```

**docker-stack.yml:**
```yaml
version: '3.8'

services:
  tirotime-web:
    image: tirotime-web:latest
    ports:
      - "8080:8080"
    secrets:
      - mailjet_api_key
      - mailjet_api_secret
      - mailjet_from_email
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - Mailjet__ApiKey=/run/secrets/mailjet_api_key
      - Mailjet__ApiSecret=/run/secrets/mailjet_api_secret
      - Mailjet__FromEmail=/run/secrets/mailjet_from_email
      - Mailjet__FromName=TiroTime

secrets:
  mailjet_api_key:
    external: true
  mailjet_api_secret:
    external: true
  mailjet_from_email:
    external: true
```

**Deployment:**
```bash
docker stack deploy -c docker-stack.yml tirotime
```

### Option 3: Kubernetes Secrets

**Secrets erstellen:**
```bash
kubectl create secret generic tirotime-secrets \
  --from-literal=mailjet-api-key=1e8e74e8110e090f0cd93cc55cf50ff6 \
  --from-literal=mailjet-api-secret=532f59e14eeec44a65efbcfe7944c4be \
  --from-literal=mailjet-from-email=alex.bierhaus@abtree.de
```

**deployment.yaml:**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: tirotime-web
spec:
  replicas: 3
  selector:
    matchLabels:
      app: tirotime-web
  template:
    metadata:
      labels:
        app: tirotime-web
    spec:
      containers:
      - name: web
        image: tirotime-web:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: Mailjet__ApiKey
          valueFrom:
            secretKeyRef:
              name: tirotime-secrets
              key: mailjet-api-key
        - name: Mailjet__ApiSecret
          valueFrom:
            secretKeyRef:
              name: tirotime-secrets
              key: mailjet-api-secret
        - name: Mailjet__FromEmail
          valueFrom:
            secretKeyRef:
              name: tirotime-secrets
              key: mailjet-from-email
        - name: Mailjet__FromName
          value: "TiroTime"
```

## Azure App Service

**Konfiguration über Portal:**
1. Azure Portal → App Service → Configuration → Application Settings
2. Neue Einstellungen hinzufügen:
   - `Mailjet__ApiKey` = `1e8e74e8110e090f0cd93cc55cf50ff6`
   - `Mailjet__ApiSecret` = `532f59e14eeec44a65efbcfe7944c4be`
   - `Mailjet__FromEmail` = `alex.bierhaus@abtree.de`
   - `Mailjet__FromName` = `TiroTime`

**Oder via Azure CLI:**
```bash
az webapp config appsettings set --name tirotime-web --resource-group TiroTime \
  --settings \
  "Mailjet__ApiKey=1e8e74e8110e090f0cd93cc55cf50ff6" \
  "Mailjet__ApiSecret=532f59e14eeec44a65efbcfe7944c4be" \
  "Mailjet__FromEmail=alex.bierhaus@abtree.de" \
  "Mailjet__FromName=TiroTime"
```

## AWS Elastic Beanstalk

**.ebextensions/environment.config:**
```yaml
option_settings:
  aws:elasticbeanstalk:application:environment:
    ASPNETCORE_ENVIRONMENT: Production
    Mailjet__ApiKey: '`{"Ref": "MailjetApiKey"}`'
    Mailjet__ApiSecret: '`{"Ref": "MailjetApiSecret"}`'
    Mailjet__FromEmail: '`{"Ref": "MailjetFromEmail"}`'
    Mailjet__FromName: TiroTime
```

Secrets über AWS Systems Manager Parameter Store oder Secrets Manager verwalten.

## Linux Server (systemd)

**/etc/systemd/system/tirotime.service:**
```ini
[Unit]
Description=TiroTime Web Application
After=network.target

[Service]
WorkingDirectory=/var/www/tirotime
ExecStart=/usr/bin/dotnet /var/www/tirotime/TiroTime.Web.dll
Restart=always
RestartSec=10
SyslogIdentifier=tirotime
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=Mailjet__ApiKey=1e8e74e8110e090f0cd93cc55cf50ff6
Environment=Mailjet__ApiSecret=532f59e14eeec44a65efbcfe7944c4be
Environment=Mailjet__FromEmail=alex.bierhaus@abtree.de
Environment=Mailjet__FromName=TiroTime

[Install]
WantedBy=multi-user.target
```

**Alternativ: Environment File verwenden**
```ini
[Service]
EnvironmentFile=/etc/tirotime/secrets.env
```

**/etc/tirotime/secrets.env:**
```bash
Mailjet__ApiKey=1e8e74e8110e090f0cd93cc55cf50ff6
Mailjet__ApiSecret=532f59e14eeec44a65efbcfe7944c4be
Mailjet__FromEmail=alex.bierhaus@abtree.de
Mailjet__FromName=TiroTime
```

**Berechtigungen setzen:**
```bash
sudo chmod 600 /etc/tirotime/secrets.env
sudo chown www-data:www-data /etc/tirotime/secrets.env
```

## Sicherheitshinweise

1. **.env Dateien:** NIEMALS in Git einchecken! In `.gitignore` hinzufügen
2. **Secrets Rotation:** API Keys regelmäßig wechseln
3. **Zugriffskontrolle:** Nur notwendige Personen erhalten Zugriff auf Secrets
4. **Logging:** API Keys NICHT in Logs ausgeben
5. **HTTPS:** In Produktion immer HTTPS verwenden

## .gitignore Einträge

Stellen Sie sicher, dass folgende Einträge in `.gitignore` vorhanden sind:

```
# User Secrets
**/secrets.json

# Environment files
.env
.env.*
!.env.example

# Docker override files
docker-compose.override.yml

# Azure
*.pubxml
*.PublishSettings

# Elastic Beanstalk
.elasticbeanstalk/
```

## Verifikation

Nach dem Deployment überprüfen:

```bash
# Logs prüfen
docker logs tirotime-web

# Oder bei systemd
journalctl -u tirotime -f

# Test: E-Mail senden über Reports-Seite
# Sollte keine Fehler wie "E-Mail-Dienst ist nicht konfiguriert" zeigen
```
