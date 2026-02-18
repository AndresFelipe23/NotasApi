# NotasApi

API backend para **Anota**, una plataforma de notas, tareas, carpetas y notas rápidas con autenticación JWT.

## Stack tecnológico

- **.NET 8** (ASP.NET Core Web API)
- **SQL Server** (CloudClusters)
- **JWT** para autenticación
- **Redis** opcional para caché (cuando se configure)

## Estructura principal

- `Program.cs`  
  Configura:
  - Controllers y Swagger
  - Autenticación **JWT** (`JwtBearer`)
  - CORS para `https://anota.click` y localhost
  - Servicios y repositorios (Notas, Carpetas, Notas rápidas, Tareas, Etiquetas)
  - Caché (Redis o memoria en desarrollo)

- `appsettings.json` / `appsettings.*.json`  
  - **NO se versionan** (ignorados en `.gitignore`).
  - En el repo se puede usar `appsettings.example.json` como plantilla.

## Configuración de `appsettings`

Ejemplo de `appsettings.example.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SERVIDOR;Database=Anota;User Id=USUARIO;Password=CONTRASEÑA;...",
    "Redis": ""
  },
  "Jwt": {
    "Secret": "TU_SECRETO_LARGO_MIN_32_CARACTERES",
    "Issuer": "AnotaAPI",
    "ExpirationMinutes": "1440"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "OpenAI": {
    "ApiKey": "",
    "Model": "whisper-1"
  }
}
```

### Entornos

- **Desarrollo** (`ASPNETCORE_ENVIRONMENT=Development`)
  - `appsettings.json` + `appsettings.Development.json`
  - Se recomienda usar **User Secrets** o variables de entorno para los secretos.

- **Producción** (`ASPNETCORE_ENVIRONMENT=Production`)
  - `appsettings.json` + `appsettings.Production.json` (solo en el servidor).
  - Contiene la cadena de conexión real y el `Jwt:Secret` de producción.

## Endpoints principales

> Ver Swagger en tiempo de ejecución.

Cuando la API está levantada se puede abrir:

- Local: `https://localhost:5246/swagger`
- Producción: `https://anotaweb.work/`

Desde ahí se documentan:

- `POST /api/auth/login` / `register` / `logout`
- CRUD de **Notas**, **Carpetas**, **Notas rápidas**, **Tareas**, **Etiquetas**
- `POST /api/transcripcion` (Whisper/OpenAI) para transcribir audio

## Desarrollo local

1. Configurar `ConnectionStrings` y `Jwt` en `appsettings.Development.json` (o User Secrets).
2. Ejecutar:

```bash
dotnet restore
dotnet run
```

La API quedará en `https://localhost:5246` (según `launchSettings.json`).

## Publicación y despliegue (CloudClusters)

En el servidor (contenedor ASP.NET):

```bash
cd /cloudclusters/NotasApi
dotnet publish -c Release -o ./publish
supervisorctl restart aspnet
supervisorctl status
```

El script `/cloudclusters/config/aspnet/startup.sh` arranca la API apuntando a:

```bash
cd /cloudclusters/NotasApi/publish
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
dotnet NotasApi.dll
```

`nginx`/CloudClusters luego expone la API como `https://anotaweb.work`.

## Notas de seguridad

- **Nunca** commitear `appsettings.json` ni `appsettings.Production.json` con secretos reales.
- Regenerar claves de OpenAI y contraseñas de SQL Server si alguna vez se subieron a GitHub.
- Mantener `Jwt:Secret` estable en producción mientras los tokens sigan siendo válidos.

