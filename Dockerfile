# --- build stage ---
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Másoljuk csak a projekt fájljait a cache használatához
COPY Backend/*.csproj ./Backend/

WORKDIR /src/Backend
RUN dotnet restore "tempbackend.csproj"

# Másoljuk az összes forráskódot
COPY Backend/ ./

RUN dotnet publish "tempbackend.csproj" -c Release -o /app/out

# --- runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "tempbackend.dll"]
