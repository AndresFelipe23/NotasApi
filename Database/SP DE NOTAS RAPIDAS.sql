USE Anota;
GO

-- ============================================================================
-- 1. CREAR NOTA RÁPIDA (Post-it instantáneo)
-- Solo requiere el texto puro, sin formato ni títulos.
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_NotasRapidas_Crear
    @UsuarioId UNIQUEIDENTIFIER,
    @Contenido NVARCHAR(MAX),
    @ColorHex VARCHAR(7) = NULL, -- Por si quieres pintar el post-it en la UI
    @NuevoId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @NuevoId = NEWID();

    -- Guardamos fechas ajustadas a zona horaria (UTC-5)
    INSERT INTO NotasRapidas (Id, UsuarioId, Contenido, ColorHex, FechaCreacion, FechaActualizacion)
    VALUES (@NuevoId, @UsuarioId, @Contenido, @ColorHex, DATEADD(HOUR, -5, GETUTCDATE()), DATEADD(HOUR, -5, GETUTCDATE()));
END;
GO

-- ============================================================================
-- 2. ACTUALIZAR NOTA RÁPIDA
-- Por si el usuario edita el texto o le cambia el color de fondo.
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_NotasRapidas_Actualizar
    @Id UNIQUEIDENTIFIER,
    @UsuarioId UNIQUEIDENTIFIER,
    @Contenido NVARCHAR(MAX),
    @ColorHex VARCHAR(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE NotasRapidas
    SET 
        Contenido = @Contenido,
        ColorHex = @ColorHex,
        FechaActualizacion = DATEADD(HOUR, -5, GETUTCDATE())
    WHERE Id = @Id AND UsuarioId = @UsuarioId;
END;
GO

-- ============================================================================
-- 3. OBTENER TODAS LAS NOTAS RÁPIDAS ACTIVAS (Para el Dashboard)
-- Trae todas las que no han sido archivadas ni convertidas a notas formales.
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_NotasRapidas_ObtenerTodas
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id, 
        UsuarioId, 
        Contenido, 
        ColorHex, 
        EsArchivada,
        FechaCreacion, 
        FechaActualizacion 
    FROM NotasRapidas 
    WHERE UsuarioId = @UsuarioId AND EsArchivada = 0
    ORDER BY FechaActualizacion DESC; -- Las más recientes primero
END;
GO

-- ============================================================================
-- 4. ARCHIVAR NOTA RÁPIDA (Ocultarla/Borrado Lógico)
-- El usuario terminó con este apunte y lo limpia de su pantalla principal.
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_NotasRapidas_Archivar
    @Id UNIQUEIDENTIFIER,
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE NotasRapidas
    SET 
        EsArchivada = 1,
        FechaActualizacion = DATEADD(HOUR, -5, GETUTCDATE())
    WHERE Id = @Id AND UsuarioId = @UsuarioId;
END;
GO

-- ============================================================================
-- 5. OBTENER NOTAS RÁPIDAS ARCHIVADAS
-- Para el panel lateral de historial / notas guardadas.
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_NotasRapidas_ObtenerArchivadas
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id,
        UsuarioId,
        Contenido,
        ColorHex,
        EsArchivada,
        FechaCreacion,
        FechaActualizacion
    FROM NotasRapidas
    WHERE UsuarioId = @UsuarioId AND EsArchivada = 1
    ORDER BY FechaActualizacion DESC;
END;
GO

-- ============================================================================
-- 6. RECUPERAR NOTA RÁPIDA ARCHIVADA
-- Revierte EsArchivada a 0 para devolverla al listado principal.
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_NotasRapidas_Recuperar
    @Id UNIQUEIDENTIFIER,
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE NotasRapidas
    SET 
        EsArchivada = 0,
        FechaActualizacion = DATEADD(HOUR, -5, GETUTCDATE())
    WHERE Id = @Id AND UsuarioId = @UsuarioId AND EsArchivada = 1;
END;
GO

-- ============================================================================
-- 5. CONVERTIR NOTA RÁPIDA A NOTA COMPLETA (Transacción Maestra)
-- Toma el texto suelto, le da formato JSON y lo mueve a la tabla de Notas formales.
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_NotasRapidas_ConvertirANota
    @UsuarioId UNIQUEIDENTIFIER,
    @NotaRapidaId UNIQUEIDENTIFIER,
    @NuevaNotaId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TextoRapido NVARCHAR(MAX);

    -- 1. Buscamos el texto de la nota rápida
    SELECT @TextoRapido = Contenido 
    FROM NotasRapidas 
    WHERE Id = @NotaRapidaId AND UsuarioId = @UsuarioId AND EsArchivada = 0;

    -- Si no existe o ya se borró, detenemos el proceso
    IF @TextoRapido IS NULL
    BEGIN
        ;THROW 50002, 'La nota rápida no existe o ya fue archivada.', 1;
    END

    -- Iniciamos la transacción porque vamos a tocar dos tablas distintas
    BEGIN TRANSACTION;
    BEGIN TRY
        
        SET @NuevaNotaId = NEWID();

        -- 2. Insertamos en la tabla principal de Notas. 
        -- Usamos STRING_ESCAPE para que si el usuario escribió comillas, no se rompa el JSON.
        -- Creamos un bloque "paragraph" básico compatible con editores modernos.
        INSERT INTO Notas (Id, UsuarioId, Titulo, Resumen, ContenidoBloques)
        VALUES (
            @NuevaNotaId, 
            @UsuarioId, 
            -- Tomamos los primeros 30 caracteres para usarlos como título automático
            LEFT(@TextoRapido, 30) + '...', 
            -- Tomamos los primeros 100 caracteres como resumen
            LEFT(@TextoRapido, 100), 
            -- Formateamos el texto puro a un bloque JSON válido
            '{"blocks":[{"id":"1","type":"paragraph","data":{"text":"' + STRING_ESCAPE(@TextoRapido, 'json') + '"}}]}'
        );

        -- 3. Archivamos la Nota Rápida para que ya no salga en el listado de Post-its
        UPDATE NotasRapidas
        SET EsArchivada = 1, FechaActualizacion = DATEADD(HOUR, -5, GETUTCDATE())
        WHERE Id = @NotaRapidaId AND UsuarioId = @UsuarioId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END
        ;THROW;
    END CATCH
END;
GO

-- ============================================================================
-- 6. ELIMINAR NOTA RÁPIDA (Borrado Físico)
-- Elimina permanentemente la nota rápida de la base de datos.
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_NotasRapidas_Eliminar
    @Id UNIQUEIDENTIFIER,
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM NotasRapidas
    WHERE Id = @Id AND UsuarioId = @UsuarioId;
END;
GO
