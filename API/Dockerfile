# # Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copy the .csproj file and restore dependencies
COPY ["API/MotoDataLoggerAPI/MotoDataLoggerAPI.csproj", "."]
RUN dotnet restore

# Copy the rest of the project files
COPY API/MotoDataLoggerAPI ./
# Build the project
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "MotoDataLoggerAPI.dll"]
