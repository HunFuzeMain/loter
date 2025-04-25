# 1. Build phase: Use the .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the entire application source code into the container
COPY . .

# Restore the dependencies (and prepare the application)
RUN dotnet restore

# Publish the app to the out directory in Release mode
RUN dotnet publish -c Release -o /app/out

# 2. Runtime phase: Use the .NET runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

# Copy the published app from the build phase
COPY --from=build /app/out ./

# The entry point of the app
ENTRYPOINT ["dotnet", "tempbackend.dll"]
