USE Anota;
GO

-- ============================================================================
-- 1. CREAR TAREA NUEVA
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Tareas_Crear
    @UsuarioId UNIQUEIDENTIFIER,
    @Descripcion NVARCHAR(500),
    @NotaVinculadaId UNIQUEIDENTIFIER = NULL, -- Opcional: Si la tarea nació dentro de una nota
    @Prioridad INT = 2, -- Por defecto 2 (Media). 1 es Alta, 3 es Baja.
    @FechaVencimiento DATETIMEOFFSET = NULL, -- Opcional: El usuario elige cuándo vence
    @NuevoId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @NuevoId = NEWID();

    -- Obtenemos el último "Orden" para que la nueva tarea aparezca al final de la lista
    DECLARE @UltimoOrden INT;
    SELECT @UltimoOrden = ISNULL(MAX(Orden), 0) 
    FROM Tareas 
    WHERE UsuarioId = @UsuarioId AND EstaCompletada = 0;

    INSERT INTO Tareas (
        Id, UsuarioId, Descripcion, NotaVinculadaId, 
        Prioridad, Orden, FechaVencimiento
    )
    VALUES (
        @NuevoId, @UsuarioId, @Descripcion, @NotaVinculadaId, 
        @Prioridad, @UltimoOrden + 1, @FechaVencimiento
    );
END;
GO

-- ============================================================================
-- 2. ACTUALIZAR TAREA (Editar texto, prioridad o fecha)
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Tareas_Actualizar
    @Id UNIQUEIDENTIFIER,
    @UsuarioId UNIQUEIDENTIFIER,
    @Descripcion NVARCHAR(500),
    @Prioridad INT,
    @FechaVencimiento DATETIMEOFFSET = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Tareas
    SET 
        Descripcion = @Descripcion,
        Prioridad = @Prioridad,
        FechaVencimiento = @FechaVencimiento
    WHERE Id = @Id AND UsuarioId = @UsuarioId;
END;
GO

-- ============================================================================
-- 3. ALTERNAR ESTADO (Check / Uncheck)
-- Marca la tarea como completada (guardando la hora) o la regresa a pendiente.
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Tareas_AlternarEstado
    @Id UNIQUEIDENTIFIER,
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Tareas
    SET 
        -- Magia de SQL: Si está en 1 pasa a 0, si está en 0 pasa a 1
        EstaCompletada = CASE WHEN EstaCompletada = 1 THEN 0 ELSE 1 END,
        
        -- Si la completamos, guardamos la fecha exacta UTC. Si la desmarcamos, la limpiamos (NULL).
        FechaCompletada = CASE WHEN EstaCompletada = 1 THEN NULL ELSE GETUTCDATE() END
    WHERE Id = @Id AND UsuarioId = @UsuarioId;
END;
GO

-- ============================================================================
-- 4. OBTENER TAREAS PENDIENTES (Para el panel lateral o Dashboard)
-- Las ordena inteligentemente para mostrar lo más urgente arriba.
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Tareas_ObtenerPendientes
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id, Descripcion, NotaVinculadaId, Prioridad, Orden, FechaVencimiento, FechaCreacion
    FROM Tareas
    WHERE UsuarioId = @UsuarioId AND EstaCompletada = 0
    ORDER BY 
        Prioridad ASC, -- 1 (Alta) va primero
        FechaVencimiento ASC, -- Las que vencen más pronto (o vencidas) suben
        Orden ASC; -- Desempata por el orden en que el usuario las arrastró
END;
GO

-- ============================================================================
-- 5. ELIMINAR TAREA (Borrado físico)
-- A diferencia de las notas, una tarea equivocada sí se suele borrar por completo.
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Tareas_Eliminar
    @Id UNIQUEIDENTIFIER,
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM Tareas
    WHERE Id = @Id AND UsuarioId = @UsuarioId;
END;
GO