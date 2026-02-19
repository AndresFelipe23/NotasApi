# 📋 Análisis: Integración Google Tasks con Anota

## 🎯 Objetivo
Permitir que los usuarios de Anota puedan ver y sincronizar sus tareas de Google Tasks dentro de la aplicación.

---

## 🔵 PARTE 1: QUÉ NECESITAS DE GOOGLE

### 1.1 Crear Proyecto en Google Cloud Console

**Pasos:**
1. Ir a [Google Cloud Console](https://console.cloud.google.com/)
2. Crear un nuevo proyecto (o usar uno existente)
3. Nombre sugerido: `Anota-Google-Integration` o similar

**Costo:** ✅ **GRATIS** (no requiere facturación para uso básico)

---

### 1.2 Habilitar Google Tasks API

**Pasos:**
1. En el proyecto, ir a **"APIs & Services" > "Library"**
2. Buscar **"Google Tasks API"**
3. Hacer clic en **"Enable"**

**Costo:** ✅ **GRATIS** (cuota gratuita: 1,000,000 requests/día)

---

### 1.3 Configurar Pantalla de Consentimiento OAuth 2.0

**Pasos:**
1. Ir a **"APIs & Services" > "OAuth consent screen"**
2. Seleccionar tipo de usuario:
   - **External** (si quieres que cualquier usuario de Google pueda conectar)
   - **Internal** (solo para usuarios de tu organización Google Workspace)
3. Completar información:
   - **App name:** `Anota`
   - **User support email:** Tu correo
   - **Developer contact:** Tu correo
   - **App logo:** (opcional) Logo de Anota
   - **App domain:** Tu dominio (ej: `anota.app`)
   - **Authorized domains:** Tu dominio
   - **Privacy policy URL:** (opcional pero recomendado)
   - **Terms of service URL:** (opcional)

4. **Scopes** (permisos que solicitarás):
   - ✅ `https://www.googleapis.com/auth/tasks` (leer y escribir tareas)
   - ✅ `https://www.googleapis.com/auth/userinfo.email` (obtener email del usuario)
   - ✅ `https://www.googleapis.com/auth/userinfo.profile` (obtener nombre del usuario)

5. **Test users** (si está en modo "Testing"):
   - Agregar correos de prueba para probar antes de publicar

**Estado de publicación:**
- **Testing:** Solo usuarios agregados pueden conectar (para desarrollo)
- **In production:** Cualquier usuario puede conectar (requiere verificación de Google si solicitas scopes sensibles)

**Costo:** ✅ **GRATIS**

---

### 1.4 Crear Credenciales OAuth 2.0

**Pasos:**
1. Ir a **"APIs & Services" > "Credentials"**
2. Clic en **"Create Credentials" > "OAuth client ID"**
3. Tipo de aplicación: **"Web application"**
4. Configurar:
   - **Name:** `Anota Web Client`
   - **Authorized JavaScript origins:**
     - `http://localhost:5173` (desarrollo)
     - `https://tu-dominio.com` (producción)
   - **Authorized redirect URIs:**
     - `http://localhost:5173/api/auth/google/callback` (desarrollo)
     - `https://tu-dominio.com/api/auth/google/callback` (producción)
     - O mejor: `https://tu-api.com/api/integrations/google/callback` (si el callback es en el backend)

5. **Guardar** y copiar:
   - **Client ID:** `123456789-abcdefghijklmnop.apps.googleusercontent.com`
   - **Client Secret:** `GOCSPX-xxxxxxxxxxxxx`

**⚠️ IMPORTANTE:**
- El **Client Secret** solo se muestra UNA VEZ. Guárdalo de forma segura.
- En producción, usa variables de entorno, nunca lo subas a Git.

**Costo:** ✅ **GRATIS**

---

### 1.5 Resumen: Qué guardar de Google

**Variables de entorno que necesitarás:**
```env
GOOGLE_CLIENT_ID=123456789-abcdefghijklmnop.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=GOCSPX-xxxxxxxxxxxxx
GOOGLE_REDIRECT_URI=https://tu-api.com/api/integrations/google/callback
```

**Scopes necesarios:**
```
https://www.googleapis.com/auth/tasks
https://www.googleapis.com/auth/userinfo.email
https://www.googleapis.com/auth/userinfo.profile
```

---

## 🗄️ PARTE 2: CAMBIOS EN BASE DE DATOS

### 2.1 Nueva Tabla: `IntegracionesGoogle`

**Propósito:** Guardar tokens de OAuth de cada usuario que conecta Google Tasks.

```sql
CREATE TABLE IntegracionesGoogle (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UsuarioId UNIQUEIDENTIFIER NOT NULL,
    
    -- Tokens OAuth
    AccessToken NVARCHAR(MAX) NOT NULL, -- Token de acceso (corto plazo, ~1 hora)
    RefreshToken NVARCHAR(MAX) NOT NULL, -- Token de refresco (largo plazo, no expira)
    TokenExpiresAt DATETIMEOFFSET NOT NULL, -- Cuándo expira el access token
    
    -- Información de Google
    GoogleUserId NVARCHAR(255) NULL, -- ID único del usuario en Google
    GoogleEmail NVARCHAR(255) NULL, -- Email de Google (para mostrar)
    GoogleName NVARCHAR(255) NULL, -- Nombre del usuario en Google
    
    -- Estado
    EstaActiva BIT DEFAULT 1, -- Si la integración está activa
    FechaConectada DATETIMEOFFSET DEFAULT GETUTCDATE(),
    FechaActualizacion DATETIMEOFFSET DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_IntegracionesGoogle_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_IntegracionesGoogle_UsuarioId UNIQUE (UsuarioId) -- Un usuario solo puede tener una integración activa
);
GO

CREATE INDEX IX_IntegracionesGoogle_UsuarioId ON IntegracionesGoogle(UsuarioId);
CREATE INDEX IX_IntegracionesGoogle_EstaActiva ON IntegracionesGoogle(UsuarioId) WHERE EstaActiva = 1;
GO
```

**Notas:**
- `RefreshToken` es **permanente** (no expira) pero puede ser revocado por el usuario.
- `AccessToken` expira en ~1 hora, se renueva automáticamente con `RefreshToken`.
- Un usuario solo puede tener **una integración activa** (puedes cambiar esto si quieres múltiples cuentas).

---

### 2.2 Nueva Tabla: `TareasGoogle` (Opcional - para sincronización bidireccional)

**Propósito:** Mapear tareas de Anota con tareas de Google Tasks para sincronización.

```sql
CREATE TABLE TareasGoogle (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TareaId UNIQUEIDENTIFIER NOT NULL, -- FK a Tareas de Anota
    UsuarioId UNIQUEIDENTIFIER NOT NULL,
    
    -- IDs de Google Tasks
    GoogleTaskListId NVARCHAR(255) NOT NULL, -- ID de la lista de tareas en Google
    GoogleTaskId NVARCHAR(255) NOT NULL, -- ID de la tarea específica en Google
    
    -- Metadatos
    UltimaSincronizacion DATETIMEOFFSET DEFAULT GETUTCDATE(),
    SincronizarDesdeGoogle BIT DEFAULT 1, -- Si cambios en Google deben actualizar Anota
    SincronizarHaciaGoogle BIT DEFAULT 1, -- Si cambios en Anota deben actualizar Google
    
    CONSTRAINT FK_TareasGoogle_Tareas FOREIGN KEY (TareaId) REFERENCES Tareas(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TareasGoogle_Usuarios FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_TareasGoogle_TareaId UNIQUE (TareaId), -- Una tarea de Anota solo puede estar vinculada a una tarea de Google
    CONSTRAINT UQ_TareasGoogle_GoogleTask UNIQUE (UsuarioId, GoogleTaskListId, GoogleTaskId) -- Una tarea de Google solo puede estar vinculada a una tarea de Anota
);
GO

CREATE INDEX IX_TareasGoogle_TareaId ON TareasGoogle(TareaId);
CREATE INDEX IX_TareasGoogle_UsuarioId ON TareasGoogle(UsuarioId);
CREATE INDEX IX_TareasGoogle_GoogleTask ON TareasGoogle(UsuarioId, GoogleTaskListId, GoogleTaskId);
GO
```

**Nota:** Esta tabla solo es necesaria si quieres **sincronización bidireccional**. Si solo quieres **mostrar** tareas de Google (lectura), no la necesitas.

---

## 🔧 PARTE 3: CAMBIOS EN EL BACKEND (NotasApi)

### 3.1 Configuración en `appsettings.json`

```json
{
  "Google": {
    "ClientId": "123456789-abcdefghijklmnop.apps.googleusercontent.com",
    "ClientSecret": "GOCSPX-xxxxxxxxxxxxx",
    "RedirectUri": "https://tu-api.com/api/integrations/google/callback",
    "Scopes": [
      "https://www.googleapis.com/auth/tasks",
      "https://www.googleapis.com/auth/userinfo.email",
      "https://www.googleapis.com/auth/userinfo.profile"
    ]
  }
}
```

**En producción:** Usa `appsettings.Production.json` o variables de entorno.

---

### 3.2 Nuevos Paquetes NuGet

```xml
<!-- En NotasApi.csproj -->
<PackageReference Include="Google.Apis.Tasks.v1" Version="1.68.0.3500" />
<PackageReference Include="Google.Apis.Auth" Version="1.68.0" />
```

O instalar vía CLI:
```bash
dotnet add package Google.Apis.Tasks.v1
dotnet add package Google.Apis.Auth
```

---

### 3.3 Estructura de Carpetas Sugerida

```
NotasApi/
├── Controllers/
│   └── IntegracionesGoogleController.cs  (NUEVO)
├── Services/
│   ├── IGoogleTasksService.cs            (NUEVO)
│   └── GoogleTasksService.cs             (NUEVO)
├── Repositories/
│   ├── IIntegracionGoogleRepository.cs   (NUEVO)
│   └── IntegracionGoogleRepository.cs    (NUEVO)
├── Models/
│   └── IntegracionGoogle.cs              (NUEVO)
└── DTOs/
    └── IntegracionesGoogle/
        ├── IniciarOAuthRequest.cs        (NUEVO)
        ├── CallbackOAuthRequest.cs       (NUEVO)
        └── ListarTareasGoogleResponse.cs (NUEVO)
```

---

### 3.4 Flujo OAuth 2.0 (Pasos)

**Paso 1: Usuario hace clic en "Conectar Google Tasks"**
- Frontend llama a: `GET /api/integrations/google/auth-url`
- Backend genera URL de autorización de Google y la devuelve
- Frontend redirige al usuario a esa URL

**Paso 2: Usuario autoriza en Google**
- Google muestra pantalla de consentimiento
- Usuario acepta permisos
- Google redirige a: `https://tu-api.com/api/integrations/google/callback?code=XXXXX&state=YYYYY`

**Paso 3: Backend intercambia código por tokens**
- Backend recibe el `code` y `state`
- Backend llama a Google para intercambiar `code` por `access_token` y `refresh_token`
- Backend guarda tokens en `IntegracionesGoogle`
- Backend redirige al frontend con éxito/error

**Paso 4: Usar tokens para llamar a Tasks API**
- Cuando el usuario quiere ver tareas de Google, backend usa `access_token`
- Si `access_token` expiró, backend usa `refresh_token` para obtener uno nuevo
- Backend llama a `https://tasks.googleapis.com/tasks/v1/users/@me/lists` y `https://tasks.googleapis.com/tasks/v1/lists/{listId}/tasks`

---

### 3.5 Endpoints Necesarios

```
GET  /api/integrations/google/auth-url
     → Devuelve URL para iniciar OAuth

GET  /api/integrations/google/callback?code=XXX&state=YYY
     → Recibe código de Google, intercambia por tokens, guarda en BD

GET  /api/integrations/google/status
     → Verifica si el usuario tiene Google conectado

DELETE /api/integrations/google/disconnect
     → Desconecta Google (borra tokens de BD)

GET  /api/integrations/google/tasks
     → Lista tareas de Google Tasks del usuario

GET  /api/integrations/google/task-lists
     → Lista las listas de tareas de Google

POST /api/integrations/google/sync
     → Sincroniza tareas de Google con Anota (opcional)
```

---

## 🎨 PARTE 4: CAMBIOS EN EL FRONTEND (AnotaWEB)

### 4.1 Nuevo Componente: `IntegracionesGoogle.tsx`

**Ubicación:** `AnotaWEB/src/components/IntegracionesGoogle.tsx`

**Funcionalidad:**
- Botón "Conectar con Google Tasks"
- Estado de conexión (conectado / no conectado)
- Lista de tareas de Google (si está conectado)
- Botón "Desconectar"

---

### 4.2 Modificar `TareasPage.tsx`

**Cambios:**
- Agregar sección/tab para "Tareas de Google"
- Mostrar tareas de Google junto con tareas de Anota (o en pestaña separada)
- Indicador visual de qué tareas vienen de Google

---

### 4.3 Nuevos Servicios en `api.ts`

```typescript
// En AnotaWEB/src/services/api.ts

// Obtener URL de autorización
getGoogleAuthUrl(): Promise<{ authUrl: string }>

// Verificar estado de conexión
getGoogleIntegrationStatus(): Promise<{ connected: boolean, email?: string }>

// Desconectar Google
disconnectGoogle(): Promise<void>

// Listar tareas de Google
getGoogleTasks(): Promise<TareaGoogle[]>

// Listar listas de tareas de Google
getGoogleTaskLists(): Promise<TaskList[]>
```

---

### 4.4 Nuevos Tipos TypeScript

```typescript
// En AnotaWEB/src/types/api.ts

export interface TareaGoogle {
  id: string;
  title: string;
  notes?: string;
  status: 'needsAction' | 'completed';
  due?: string;
  completed?: string;
  taskListId: string;
  taskListTitle: string;
}

export interface TaskList {
  id: string;
  title: string;
}
```

---

## 📊 PARTE 5: RESUMEN DE REQUISITOS

### ✅ De Google (GRATIS):
- [x] Proyecto en Google Cloud Console
- [x] Google Tasks API habilitada
- [x] Pantalla de consentimiento OAuth configurada
- [x] Credenciales OAuth 2.0 (Client ID + Secret)
- [x] Scopes: `tasks`, `userinfo.email`, `userinfo.profile`

### ✅ Base de Datos:
- [x] Tabla `IntegracionesGoogle` (guardar tokens)
- [x] Tabla `TareasGoogle` (opcional, solo si sincronización bidireccional)

### ✅ Backend:
- [x] Paquetes NuGet: `Google.Apis.Tasks.v1`, `Google.Apis.Auth`
- [x] Configuración en `appsettings.json`
- [x] `IntegracionesGoogleController` (endpoints OAuth)
- [x] `GoogleTasksService` (llamar a Tasks API)
- [x] `IntegracionGoogleRepository` (guardar tokens)

### ✅ Frontend:
- [x] Componente `IntegracionesGoogle.tsx`
- [x] Modificar `TareasPage.tsx` para mostrar tareas de Google
- [x] Servicios en `api.ts` para llamar a endpoints de integración
- [x] Tipos TypeScript para tareas de Google

---

## 🚀 PARTE 6: ORDEN DE IMPLEMENTACIÓN SUGERIDO

### Fase 1: Configuración Google (1-2 horas)
1. Crear proyecto en Google Cloud Console
2. Habilitar Tasks API
3. Configurar OAuth consent screen
4. Crear credenciales OAuth 2.0
5. Guardar Client ID y Secret en variables de entorno

### Fase 2: Base de Datos (30 min)
1. Crear tabla `IntegracionesGoogle`
2. Crear stored procedures si es necesario
3. Crear índices

### Fase 3: Backend - OAuth Flow (2-3 horas)
1. Instalar paquetes NuGet
2. Crear modelo `IntegracionGoogle`
3. Crear `IntegracionGoogleRepository`
4. Crear `GoogleTasksService` (solo métodos OAuth primero)
5. Crear `IntegracionesGoogleController` con endpoints:
   - `GET /auth-url`
   - `GET /callback`
   - `GET /status`
   - `DELETE /disconnect`

### Fase 4: Backend - Tasks API (2-3 horas)
1. Agregar métodos en `GoogleTasksService` para:
   - Listar listas de tareas
   - Listar tareas de una lista
   - Refrescar access token automáticamente
2. Agregar endpoints:
   - `GET /task-lists`
   - `GET /tasks`

### Fase 5: Frontend (2-3 horas)
1. Crear componente `IntegracionesGoogle.tsx`
2. Agregar servicios en `api.ts`
3. Agregar tipos TypeScript
4. Modificar `TareasPage.tsx` para mostrar tareas de Google

### Fase 6: Testing (1-2 horas)
1. Probar flujo OAuth completo
2. Probar listado de tareas
3. Probar refresh de tokens
4. Probar desconexión

**Tiempo total estimado:** 8-13 horas

---

## ⚠️ CONSIDERACIONES IMPORTANTES

### Seguridad:
- **NUNCA** expongas `Client Secret` en el frontend
- El flujo OAuth debe ser **siempre** iniciado desde el backend
- Guarda `RefreshToken` encriptado en la base de datos (opcional pero recomendado)
- Valida `state` en el callback para prevenir CSRF

### Tokens:
- `AccessToken` expira en ~1 hora
- `RefreshToken` es permanente pero puede ser revocado
- Implementa refresh automático antes de que expire el `AccessToken`
- Maneja errores cuando `RefreshToken` es inválido (usuario revocó permisos)

### Límites de Google Tasks API:
- **Cuota gratuita:** 1,000,000 requests/día
- **Rate limit:** ~10 requests/segundo por usuario
- Para la mayoría de casos, es más que suficiente

### Sincronización:
- **Solo lectura (más simple):** Mostrar tareas de Google sin guardarlas en Anota
- **Sincronización (más complejo):** Guardar tareas de Google en tabla `Tareas` y mantener sincronización bidireccional
- **Recomendación inicial:** Empezar con solo lectura, luego agregar sincronización si es necesario

---

## 📚 RECURSOS ÚTILES

- [Google Tasks API Documentation](https://developers.google.com/tasks)
- [Google OAuth 2.0 Guide](https://developers.google.com/identity/protocols/oauth2)
- [Google .NET Client Library](https://github.com/googleapis/google-api-dotnet-client)
- [Google Tasks API Explorer](https://developers.google.com/tasks/api/reference/rest/v1/tasks/list)

---

## ✅ SIGUIENTE PASO

Una vez que tengas:
1. ✅ Proyecto creado en Google Cloud Console
2. ✅ Tasks API habilitada
3. ✅ Credenciales OAuth creadas (Client ID + Secret)

Podemos empezar a implementar el código del backend y frontend.
