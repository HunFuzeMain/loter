# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy only your project file first and restore dependencies
# (replace tempbackend.csproj with your actual .csproj name)
COPY Backend/tempbackend.csproj ./
RUN dotnet restore tempbackend.csproj

# Now copy the rest of your source
COPY Backend/ ./

# Publish the app into /app/out
RUN dotnet publish tempbackend.csproj -c Release -o /app/out

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

# Copy the published output
COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "tempbackend.dll"]
