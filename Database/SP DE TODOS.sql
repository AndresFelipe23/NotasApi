USE Anota;
GO

-- ============================================================================
-- 1. CREAR TAREA NUEVA
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Tareas_Crear
    @UsuarioId UNIQUEIDENTIFIER,
    @Descripcion NVARCHAR(500),
    @NotaVinculadaId UNIQUEIDENTIFIER = NULL, -- Opcional: Si la tarea naci? dentro de una nota
    @Prioridad INT = 2, -- Por defecto 2 (Media). 1 es Alta, 3 es Baja.
    @FechaVencimiento DATETIMEOFFSET = NULL, -- Opcional: El usuario elige cu?ndo vence
    @NuevoId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @NuevoId = NEWID();

    -- Obtenemos el ?ltimo "Orden" para que la nueva tarea aparezca al final de la lista
    DECLARE @UltimoOrden INT;
    SELECT @UltimoOrden = ISNULL(MAX(Orden), 0) 
    FROM Tareas 
    WHERE UsuarioId = @UsuarioId AND EstaCompletada = 0;

    INSERT INTO Tareas (
        Id, UsuarioId, Descripcion, NotaVinculadaId, 
        Prioridad, Orden, FechaVencimiento, FechaCreacion
    )
    VALUES (
        @NuevoId, @UsuarioId, @Descripcion, @NotaVinculadaId, 
        @Prioridad, @UltimoOrden + 1, @FechaVencimiento, DATEADD(HOUR, -5, GETUTCDATE())
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
        -- Magia de SQL: Si est? en 1 pasa a 0, si est? en 0 pasa a 1
        EstaCompletada = CASE WHEN EstaCompletada = 1 THEN 0 ELSE 1 END,
        
        -- Si la completamos, guardamos la fecha Colombia (-5). Si la desmarcamos, la limpiamos (NULL).
        FechaCompletada = CASE WHEN EstaCompletada = 1 THEN NULL ELSE DATEADD(HOUR, -5, GETUTCDATE()) END
    WHERE Id = @Id AND UsuarioId = @UsuarioId;
END;
GO

-- ============================================================================
-- 4. OBTENER TAREAS PENDIENTES (Para el panel lateral o Dashboard)
-- Las ordena inteligentemente para mostrar lo m?s urgente arriba.
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Tareas_ObtenerPendientes
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        t.Id, t.Descripcion, t.NotaVinculadaId, t.Prioridad, t.Orden, t.FechaVencimiento,
        DATEADD(HOUR, -5, t.FechaCreacion) AS FechaCreacion,
        n.Titulo AS TituloNotaVinculada
    FROM Tareas t
    LEFT JOIN Notas n ON t.NotaVinculadaId = n.Id
    WHERE t.UsuarioId = @UsuarioId AND t.EstaCompletada = 0
    ORDER BY 
        t.Prioridad ASC,
        t.FechaVencimiento ASC,
        t.Orden ASC,
        t.FechaCreacion ASC;
END;
GO

-- ============================================================================
-- 4b. OBTENER TAREAS COMPLETADAS
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Tareas_ObtenerCompletadas
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        t.Id, t.Descripcion, t.NotaVinculadaId, t.Prioridad, t.Orden, 
        t.EstaCompletada, t.FechaVencimiento,
        DATEADD(HOUR, -5, t.FechaCreacion) AS FechaCreacion,
        DATEADD(HOUR, -5, t.FechaCompletada) AS FechaCompletada,
        n.Titulo AS TituloNotaVinculada
    FROM Tareas t
    LEFT JOIN Notas n ON t.NotaVinculadaId = n.Id
    WHERE t.UsuarioId = @UsuarioId AND t.EstaCompletada = 1
    ORDER BY t.FechaCompletada DESC;
END;
GO

-- ============================================================================
-- 4c. OBTENER TAREAS POR NOTA VINCULADA (Para mostrar en la vista de una nota)
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Tareas_ObtenerPorNotaVinculada
    @UsuarioId UNIQUEIDENTIFIER,
    @NotaVinculadaId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        t.Id, t.Descripcion, t.NotaVinculadaId, t.Prioridad, t.Orden, t.EstaCompletada,
        t.FechaVencimiento,
        DATEADD(HOUR, -5, t.FechaCreacion) AS FechaCreacion,
        DATEADD(HOUR, -5, t.FechaCompletada) AS FechaCompletada,
        n.Titulo AS TituloNotaVinculada
    FROM Tareas t
    LEFT JOIN Notas n ON t.NotaVinculadaId = n.Id
    WHERE t.UsuarioId = @UsuarioId AND t.NotaVinculadaId = @NotaVinculadaId
    ORDER BY t.EstaCompletada ASC, t.Prioridad ASC, t.FechaVencimiento ASC, t.Orden ASC;
END;
GO

-- ============================================================================
-- 5. ELIMINAR TAREA (Borrado f?sico)
-- A diferencia de las notas, una tarea equivocada s? se suele borrar por completo.
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