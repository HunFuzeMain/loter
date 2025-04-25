# 1. Build kép
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Mappa létrehozása és beállítása
WORKDIR /app

# Másoljuk a projekt fájlokat
COPY Backend/*.csproj ./

# Restaurálás
RUN dotnet restore tempbackend.csproj

# Másoljuk az összes fájlt a konténerbe
COPY Backend/ ./

# Publish a Release módba
RUN dotnet publish tempbackend.csproj -c Release -o /app/out

# 2. Runtime kép
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

# A kiadott fájlok átmásolása
COPY --from=build /app/out ./

# Az indításhoz szükséges fájlok beállítása
ENTRYPOINT ["dotnet", "tempbackend.dll"]
