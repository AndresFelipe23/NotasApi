# NotaVinculadaId en la tabla Tareas

## ¿Qué es?

`NotaVinculadaId` es una columna **opcional** (nullable) en la tabla **Tareas** que guarda el **Id de una Nota** a la que la tarea está vinculada. Es una clave foránea a `Notas(Id)`.

- Si tiene valor: la tarea "nació" o está asociada a esa nota.
- Si es NULL: la tarea es independiente (creada desde la vista de Tareas sin vincular a ninguna nota).

## ¿Para qué sirve?

Permite **relacionar tareas con notas**: una tarea puede estar ligada a una nota concreta (por ejemplo, una tarea extraída de una lista dentro de esa nota). Así se mantiene el contexto: "esta tarea pertenece a esta nota".

## Funcionalidad para el usuario

### 1. Crear tareas desde una nota

- Dentro del **editor de una nota** (Notas tipo Notion), el usuario puede crear ítems de lista o checklist que se guardan como **Tareas** en la base de datos.
- Al crear esas tareas, se envía `NotaVinculadaId = id de la nota actual`.
- El usuario ve sus tareas en la vista **Tareas** y sabe que cada una "viene" de una nota concreta.

### 2. Ver el origen en la vista Tareas

- En la página **Tareas**, cada tarea que tenga `NotaVinculadaId` puede mostrar un indicador o enlace: "Vinculada a: [título de la nota]".
- Al hacer clic en ese enlace, se abre la nota vinculada para ver el contexto completo.

### 3. Crear tareas sueltas (sin nota)

- Si el usuario crea una tarea desde el formulario de la página **Tareas** (sin estar dentro de una nota), no se envía `NotaVinculadaId` (o se envía `null`). La tarea queda "suelta" y sigue funcionando igual.

### 4. Resumen

| Origen de la tarea     | NotaVinculadaId | Experiencia del usuario                          |
|------------------------|------------------|--------------------------------------------------|
| Creada desde una nota  | Id de esa nota   | En Tareas se ve "Vinculada a: [nombre nota]" y puede abrir la nota. |
| Creada desde /tareas   | NULL             | Tarea independiente, sin enlace a ninguna nota. |

## Estado actual en el producto

- **Base de datos y API:** La columna existe, el SP `usp_Tareas_Crear` acepta `@NotaVinculadaId` y el modelo/DTO incluyen `NotaVinculadaId`.
- **Front (AnotaWEB):** El tipo `Tarea` tiene `notaVinculadaId` opcional y `CrearTareaRequest` puede enviarlo, pero hoy la UI no suele enviarlo al crear tareas ni muestra "vinculada a nota" en la lista. Para explotar la funcionalidad al 100% faltaría:
  1. En el **editor de notas**: poder crear ítems de tarea y llamar a la API con `notaVinculadaId` de la nota actual.
  2. En la **vista Tareas**: mostrar el enlace a la nota cuando `notaVinculadaId` exista y, si la API lo devuelve, el título de la nota (o un endpoint que devuelva nota + tareas vinculadas).

## Ejemplo de flujo completo

1. Usuario abre la nota "Plan proyecto X".
2. Dentro de la nota añade una lista de tareas: "Revisar presupuesto", "Enviar propuesta".
3. La app crea dos registros en **Tareas** con `NotaVinculadaId = Id de "Plan proyecto X"`.
4. En **Tareas**, el usuario ve esas dos tareas con un texto tipo "Nota: Plan proyecto X" y un botón "Abrir nota".
5. Así puede hacer el seguimiento desde Tareas pero no pierde el contexto de la nota.
