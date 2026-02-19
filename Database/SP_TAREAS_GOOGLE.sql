-- =========================================
-- STORED PROCEDURES: TareasGoogle (Sincronización)
-- =========================================
USE Anota;
GO

-- ============================================================================
-- 1. VINCULAR TAREA DE ANOTA CON TAREA DE GOOGLE
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_TareasGoogle_Vincular
    @TareaId UNIQUEIDENTIFIER,
    @UsuarioId UNIQUEIDENTIFIER,
    @GoogleTaskListId NVARCHAR(255),
    @GoogleTaskId NVARCHAR(255),
    @SincronizarDesdeGoogle BIT = 1,
    @SincronizarHaciaGoogle BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Verificar que la tarea pertenece al usuario
    IF NOT EXISTS (SELECT 1 FROM Tareas WHERE Id = @TareaId AND UsuarioId = @UsuarioId)
    BEGIN
        THROW 50000, 'La tarea no pertenece al usuario especificado', 1;
        RETURN;
    END
    
    IF EXISTS (SELECT 1 FROM TareasGoogle WHERE TareaId = @TareaId)
    BEGIN
        -- Actualizar vinculación existente
        UPDATE TareasGoogle
        SET 
            GoogleTaskListId = @GoogleTaskListId,
            GoogleTaskId = @GoogleTaskId,
            SincronizarDesdeGoogle = @SincronizarDesdeGoogle,
            SincronizarHaciaGoogle = @SincronizarHaciaGoogle,
            UltimaSincronizacion = GETUTCDATE()
        WHERE TareaId = @TareaId;
    END
    ELSE
    BEGIN
        -- Crear nueva vinculación
        INSERT INTO TareasGoogle (
            TareaId, UsuarioId, GoogleTaskListId, GoogleTaskId,
            SincronizarDesdeGoogle, SincronizarHaciaGoogle
        )
        VALUES (
            @TareaId, @UsuarioId, @GoogleTaskListId, @GoogleTaskId,
            @SincronizarDesdeGoogle, @SincronizarHaciaGoogle
        );
    END
END;
GO

-- ============================================================================
-- 2. OBTENER VINCULACIÓN POR TAREA DE ANOTA
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_TareasGoogle_ObtenerPorTarea
    @TareaId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        TareaId,
        UsuarioId,
        GoogleTaskListId,
        GoogleTaskId,
        UltimaSincronizacion,
        SincronizarDesdeGoogle,
        SincronizarHaciaGoogle,
        UltimaModificacionEnGoogle,
        UltimaModificacionEnAnota
    FROM TareasGoogle
    WHERE TareaId = @TareaId;
END;
GO

-- ============================================================================
-- 3. OBTENER VINCULACIÓN POR TAREA DE GOOGLE
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_TareasGoogle_ObtenerPorGoogleTask
    @UsuarioId UNIQUEIDENTIFIER,
    @GoogleTaskListId NVARCHAR(255),
    @GoogleTaskId NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        Id,
        TareaId,
        UsuarioId,
        GoogleTaskListId,
        GoogleTaskId,
        UltimaSincronizacion,
        SincronizarDesdeGoogle,
        SincronizarHaciaGoogle,
        UltimaModificacionEnGoogle,
        UltimaModificacionEnAnota
    FROM TareasGoogle
    WHERE UsuarioId = @UsuarioId 
        AND GoogleTaskListId = @GoogleTaskListId 
        AND GoogleTaskId = @GoogleTaskId;
END;
GO

-- ============================================================================
-- 4. OBTENER TODAS LAS VINCULACIONES DE UN USUARIO
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_TareasGoogle_ObtenerPorUsuario
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        tg.Id,
        tg.TareaId,
        tg.UsuarioId,
        tg.GoogleTaskListId,
        tg.GoogleTaskId,
        tg.UltimaSincronizacion,
        tg.SincronizarDesdeGoogle,
        tg.SincronizarHaciaGoogle,
        tg.UltimaModificacionEnGoogle,
        tg.UltimaModificacionEnAnota,
        t.Descripcion AS TareaDescripcion,
        t.EstaCompletada AS TareaEstaCompletada,
        t.FechaVencimiento AS TareaFechaVencimiento
    FROM TareasGoogle tg
    INNER JOIN Tareas t ON tg.TareaId = t.Id
    WHERE tg.UsuarioId = @UsuarioId;
END;
GO

-- ============================================================================
-- 5. ACTUALIZAR TIMESTAMP DE MODIFICACIÓN EN GOOGLE
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_TareasGoogle_ActualizarModificacionGoogle
    @TareaId UNIQUEIDENTIFIER,
    @FechaModificacion DATETIMEOFFSET
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE TareasGoogle
    SET 
        UltimaModificacionEnGoogle = @FechaModificacion,
        UltimaSincronizacion = GETUTCDATE()
    WHERE TareaId = @TareaId;
END;
GO

-- ============================================================================
-- 6. ACTUALIZAR TIMESTAMP DE MODIFICACIÓN EN ANOTA
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_TareasGoogle_ActualizarModificacionAnota
    @TareaId UNIQUEIDENTIFIER,
    @FechaModificacion DATETIMEOFFSET
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE TareasGoogle
    SET 
        UltimaModificacionEnAnota = @FechaModificacion,
        UltimaSincronizacion = GETUTCDATE()
    WHERE TareaId = @TareaId;
END;
GO

-- ============================================================================
-- 7. DESVINCULAR TAREA (eliminar relación)
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_TareasGoogle_Desvincular
    @TareaId UNIQUEIDENTIFIER,
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM TareasGoogle
    WHERE TareaId = @TareaId AND UsuarioId = @UsuarioId;
END;
GO

-- ============================================================================
-- 8. OBTENER TAREAS PENDIENTES DE SINCRONIZACIÓN
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_TareasGoogle_ObtenerPendientesSincronizacion
    @UsuarioId UNIQUEIDENTIFIER,
    @SincronizarHaciaGoogle BIT = NULL -- NULL = todas, 1 = solo las que deben sincronizarse hacia Google
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        tg.Id,
        tg.TareaId,
        tg.UsuarioId,
        tg.GoogleTaskListId,
        tg.GoogleTaskId,
        tg.UltimaSincronizacion,
        tg.SincronizarDesdeGoogle,
        tg.SincronizarHaciaGoogle,
        tg.UltimaModificacionEnGoogle,
        tg.UltimaModificacionEnAnota,
        t.Descripcion,
        t.EstaCompletada,
        t.Prioridad,
        t.FechaVencimiento,
        t.FechaCreacion,
        t.FechaCompletada
    FROM TareasGoogle tg
    INNER JOIN Tareas t ON tg.TareaId = t.Id
    WHERE tg.UsuarioId = @UsuarioId
        AND (@SincronizarHaciaGoogle IS NULL OR tg.SincronizarHaciaGoogle = @SincronizarHaciaGoogle)
    ORDER BY tg.UltimaSincronizacion ASC;
END;
GO
