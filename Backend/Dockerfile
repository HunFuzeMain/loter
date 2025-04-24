# 1. build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Másoljuk be a csak a csproj-ot, és futtassuk le a restore-t (cache-elhetõ)
COPY ["tempbackend.csproj", "./"]
RUN dotnet restore "tempbackend.csproj"

# Másoljuk be az egész kódot, és publish-oljunk Release konfigurációban
COPY . .
RUN dotnet publish "tempbackend.csproj" -c Release -o /app --no-restore

# 2. runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

# Másoljuk át a buildelt kimenetet
COPY --from=build /app ./

# A konténer alapértelmezett entrypoint-je
ENTRYPOINT ["dotnet", "tempbackend.dll"]
