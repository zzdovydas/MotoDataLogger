# # Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app/MotoDataLoggerAPI

# Copy the .csproj file and restore dependencies
COPY ["API/MotoDataLoggerAPI/MotoDataLoggerAPI.csproj", "."]
RUN dotnet restore

# Copy the rest of the project files
COPY API/MotoDataLoggerAPI ./
# Build the project
RUN dotnet publish -c Release -o out

# Run tests
WORKDIR /app/MotoDataLoggerAPI.Tests
COPY API/MotoDataLoggerAPI.Tests ./
RUN dotnet test

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app/MotoDataLoggerAPI
COPY --from=build-env /app/MotoDataLoggerAPI/out .
ENTRYPOINT ["dotnet", "MotoDataLoggerAPI.dll"]
