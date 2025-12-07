# Stage 1: Build
# Use Alpine SDK for smaller build image context
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy solution and project files first to leverage Docker layer caching
COPY Velum.slnx ./
COPY api/Velum.Api.csproj api/
COPY base/Velum.Base.csproj base/
COPY core/Velum.Core.csproj core/

# Restore dependencies with the specific runtime identifier for Alpine
RUN dotnet restore api/Velum.Api.csproj -r linux-musl-x64

# Copy the remaining source code
COPY . .

WORKDIR /src/api
RUN dotnet publish -c Release -o /app/publish \
    -r linux-musl-x64 \
    --no-restore \
    --self-contained false

# Stage 2: Runtime
# Use Alpine Runtime for minimal image size (Efficiency)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# Install ICU libraries for Globalization support (Alpine requirement for .NET)
# This ensures correct date/time/currency formatting
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Expose the application port
EXPOSE 17597
ENV ASPNETCORE_URLS=http://+:17597

# Create a data directory and ensure the app user owns it
# This is required for SQLite to write the database file
USER root
RUN mkdir -p /data && chown -R app:app /data

# Use a non-root user for security
USER app

ENTRYPOINT ["dotnet", "Velum.Api.dll"]
