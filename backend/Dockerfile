# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the solution file and project files
COPY Velum.slnx ./
COPY api/Velum.Api.csproj api/
COPY base/Velum.Base.csproj base/
COPY core/Velum.Core.csproj core/

# Restore dependencies
RUN dotnet restore api/Velum.Api.csproj

# Copy the rest of the source code
COPY . .

# Build and publish the application
WORKDIR /src/api
RUN dotnet publish -c Release -o /app/publish

# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose the port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Run the application
ENTRYPOINT ["dotnet", "Velum.Api.dll"]
