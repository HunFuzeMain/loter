# 1) Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy only the project file, restore deps
COPY Backend/*.csproj ./
RUN dotnet restore

# Copy everything else and publish
COPY Backend/. ./
RUN dotnet publish -c Release -o /app/publish

# 2) Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/publish ./

# The DLL name must match your project’s output
ENTRYPOINT ["dotnet", "tempbackend.dll"]
