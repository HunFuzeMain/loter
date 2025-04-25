# Use the official .NET SDK image as the base image for building the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Set the working directory in the container
WORKDIR /app

# Copy the .csproj file and restore any dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application files into the container
COPY . ./

# Build the application in Release mode
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final

# Set the working directory in the container for running the app
WORKDIR /app

# Copy the published app from the build image
COPY --from=build /app/out ./

# Set the entry point for the container
ENTRYPOINT ["dotnet", "tempbackend.dll"]
