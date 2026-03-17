# Etapa base para el runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el archivo de proyecto principal y restaurar dependencias
# (El contexto de build es el directorio raíz del proyecto)
COPY ["GACSE/GACSE.csproj", "GACSE/"]
RUN dotnet restore "GACSE/GACSE.csproj"

# Copiar todo el código fuente
COPY . .

# Compilar y publicar
WORKDIR "/src/GACSE"
RUN dotnet publish "GACSE.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final: runtime + archivos publicados
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Copiar el frontend estático a wwwroot (visto desde el contexto del root)
COPY ["GACSE FRONT END/", "./wwwroot/"]

ENTRYPOINT ["dotnet", "GACSE.dll"]