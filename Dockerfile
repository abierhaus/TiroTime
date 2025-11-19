# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution file
COPY TiroTime.sln .

# Copy project files (including test projects for restore)
COPY src/TiroTime.Domain/TiroTime.Domain.csproj src/TiroTime.Domain/
COPY src/TiroTime.Application/TiroTime.Application.csproj src/TiroTime.Application/
COPY src/TiroTime.Infrastructure/TiroTime.Infrastructure.csproj src/TiroTime.Infrastructure/
COPY src/TiroTime.Web/TiroTime.Web.csproj src/TiroTime.Web/
COPY tests/TiroTime.Domain.Tests/TiroTime.Domain.Tests.csproj tests/TiroTime.Domain.Tests/
COPY tests/TiroTime.Application.Tests/TiroTime.Application.Tests.csproj tests/TiroTime.Application.Tests/

# Restore dependencies
RUN dotnet restore src/TiroTime.Web/TiroTime.Web.csproj

# Copy all source files (excluding tests)
COPY src/ src/

# Build and publish (only Web project, no tests)
WORKDIR /src/src/TiroTime.Web
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
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
