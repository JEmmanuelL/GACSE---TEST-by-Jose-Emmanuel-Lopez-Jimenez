# Dockerfile
# El build se hace localmente, Docker solo empaqueta el runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Copiar el output pre-publicado de la API
COPY GACSE/publish/ .

# Copiar el frontend estático a wwwroot
COPY ["GACSE FRONT END/", "./wwwroot/"]

ENTRYPOINT ["dotnet", "GACSE.dll"]