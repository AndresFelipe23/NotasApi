# Cómo probar la integración Google Tasks

## 1. Configuración en el servidor (anotaweb.work)

En el servidor donde corre la API (anotaweb.work) debe existir la sección **Google** en configuración:

- **appsettings.Production.json** (o variables de entorno):
  - `Google:ClientId` = tu Client ID
  - `Google:ClientSecret` = tu Client Secret
  - `Google:RedirectUri` = `https://anotaweb.work/api/integrations/google/callback`
  - `Google:FrontendBaseUrl` = `https://anota.click`

Si usas variables de entorno en lugar del archivo:
- `Google__ClientId`
- `Google__ClientSecret`
- `Google__RedirectUri`
- `Google__FrontendBaseUrl`

---

## 2. Probar en producción (recomendado)

1. **Desplegar** la API en anotaweb.work y el front en anota.click (si no lo tienes ya).
2. **Iniciar sesión** en https://anota.click con tu usuario de Anota.
3. Ir a **Tareas** (menú o ruta `/tareas`).
4. Buscar el bloque de **Google Tasks** y pulsar **“Conectar con Google Tasks”**.
5. Serás redirigido a Google. Elige la cuenta y acepta los permisos.
6. Google redirige a la API (`anotaweb.work/.../callback`) y la API te devuelve a `anota.click/tareas?google=success`.
7. Deberías ver un mensaje de éxito y, si hay listas/tareas en Google Tasks, se cargarán en la página.

**Comprobar estado:**
- Si ya está conectado, verás el email de Google y un botón para desconectar.
- Las tareas de Google se listan en la sección correspondiente.

---

## 3. Probar en local

Para probar con front en `localhost:5173` y API en `localhost:5246`:

1. **Google Cloud Console**  
   En “URIs de redireccionamiento autorizados” añade:
   - `http://localhost:5246/api/integrations/google/callback`  
   (5246 es el puerto de tu API en local; si usas otro, cámbialo.)

2. **appsettings.Development.json** en NotasApi:
   - `RedirectUri` = `http://localhost:5246/api/integrations/google/callback`
   - `FrontendBaseUrl` = `http://localhost:5173`

3. Arrancar la API (NotasApi) en el puerto 5246.
4. Arrancar el front (AnotaWEB) con `npm run dev` (puerto 5173).
5. En el navegador ir a `http://localhost:5173`, iniciar sesión, ir a Tareas y pulsar “Conectar con Google Tasks”.
6. Tras autorizar en Google, volverás a `http://localhost:5173/tareas?google=success`.

---

## 4. Errores frecuentes

| Síntoma | Qué revisar |
|--------|-----------------------------|
| “redirect_uri_mismatch” | La URI exacta que usa la API (RedirectUri) debe estar en “URIs de redireccionamiento autorizados” en Google Console (incluye http/https y puerto). |
| Callback a la API pero luego “error” en la URL | Revisar logs de la API en el callback; suele ser fallo al guardar tokens (BD o configuración). |
| No se muestran tareas de Google | Comprobar que el usuario tenga listas/tareas en Google Tasks; revisar que la API tenga Google Tasks API habilitada en el proyecto de Google Cloud. |
| 401 al pedir auth-url o status | El usuario debe estar autenticado (JWT). Probar desde la app ya logueado, no desde Swagger sin token. |

---

## 5. Probar solo la API (Swagger)

1. Abrir https://anotaweb.work (Swagger).
2. Hacer **POST /api/Auth/login** y copiar el `token` de la respuesta.
3. En Swagger, pulsar “Authorize” y pegar: `Bearer <token>`.
4. **GET /api/integrations/google/auth-url** → devuelve `authUrl`.
5. Abrir esa `authUrl` en el navegador, autorizar en Google.
6. Tras la redirección, deberías terminar en `anota.click/tareas?google=success`.
7. Con el mismo token, **GET /api/integrations/google/status** → debe devolver `connected: true`.
8. **GET /api/integrations/google/tasks** → lista de tareas de Google.

Con esto puedes validar que la integración y las tablas/procedimientos funcionan antes o junto con las pruebas en la UI.
