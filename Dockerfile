# ==================== Build Stage ====================
# Use official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies first (leverages Docker cache)
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and publish
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# ==================== Runtime Stage ====================
# Use lightweight ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish ./

# Set port for Render dynamic assignment
ENV ASPNETCORE_URLS=http://*:${PORT}

# Start the app with the correct DLL name
ENTRYPOINT ["dotnet", "EVChargingBookingAPI.dll"]
