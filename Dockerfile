# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY TiroTime.sln .

# Copy project files
COPY src/TiroTime.Domain/TiroTime.Domain.csproj src/TiroTime.Domain/
COPY src/TiroTime.Application/TiroTime.Application.csproj src/TiroTime.Application/
COPY src/TiroTime.Infrastructure/TiroTime.Infrastructure.csproj src/TiroTime.Infrastructure/
COPY src/TiroTime.Web/TiroTime.Web.csproj src/TiroTime.Web/

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY src/ src/

# Build and publish
WORKDIR /src/src/TiroTime.Web
RUN dotnet publish -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

# Expose ports
EXPOSE 80
EXPOSE 443

# Set environment to Production
ENV ASPNETCORE_ENVIRONMENT=Production

# Start the application
ENTRYPOINT ["dotnet", "TiroTime.Web.dll"]
