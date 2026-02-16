USE Anota;
GO

-- ============================================================================
-- 1. CREAR NOTA NUEVA
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Notas_Crear
    @UsuarioId UNIQUEIDENTIFIER,
    @CarpetaId UNIQUEIDENTIFIER = NULL, -- NULL significa que va a la raíz
    @Titulo NVARCHAR(200),
    @Resumen NVARCHAR(300) = NULL,
    @Icono NVARCHAR(50) = NULL,
    @ContenidoBloques NVARCHAR(MAX) = NULL, -- El JSON inicial (puede venir vacío)
    @NuevoId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @NuevoId = NEWID();

    INSERT INTO Notas (
        Id, UsuarioId, CarpetaId, Titulo, Resumen, 
        Icono, ContenidoBloques
    )
    VALUES (
        @NuevoId, @UsuarioId, @CarpetaId, @Titulo, @Resumen, 
        @Icono, @ContenidoBloques
    );
END;
GO

-- ============================================================================
-- 2. ACTUALIZAR NOTA (Ideal para el Autoguardado del Editor)
-- Actualiza el contenido y cambia automáticamente la FechaActualizacion
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Notas_Actualizar
    @Id UNIQUEIDENTIFIER,
    @UsuarioId UNIQUEIDENTIFIER,
    @Titulo NVARCHAR(200),
    @Resumen NVARCHAR(300) = NULL,
    @Icono NVARCHAR(50) = NULL,
    @ImagenPortadaUrl NVARCHAR(500) = NULL,
    @ContenidoBloques NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Notas
    SET 
        Titulo = @Titulo,
        Resumen = @Resumen,
        Icono = @Icono,
        ImagenPortadaUrl = @ImagenPortadaUrl,
        ContenidoBloques = @ContenidoBloques,
        FechaActualizacion = GETUTCDATE()
    WHERE Id = @Id AND UsuarioId = @UsuarioId;
END;
GO

-- ============================================================================
-- 3. OBTENER NOTA COMPLETA (Lectura Pesada)
-- Se ejecuta SOLO cuando el usuario hace clic para abrir la nota en el editor
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Notas_ObtenerPorId
    @Id UNIQUEIDENTIFIER,
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id, CarpetaId, Titulo, Resumen, Icono, ImagenPortadaUrl, 
        ContenidoBloques, EsFavorita, EsArchivada, EsPublica, 
        FechaCreacion, FechaActualizacion
    FROM Notas
    WHERE Id = @Id AND UsuarioId = @UsuarioId AND EsArchivada = 0;
END;
GO

-- ============================================================================
-- 4. OBTENER RESUMEN POR CARPETA (Lectura Ligera)
-- Devuelve la lista de notas SIN el JSON pesado. Ideal para el menú lateral.
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Notas_ObtenerResumenPorCarpeta
    @UsuarioId UNIQUEIDENTIFIER,
    @CarpetaId UNIQUEIDENTIFIER = NULL 
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id, Titulo, Resumen, Icono, ImagenPortadaUrl, 
        EsFavorita, EsPublica, FechaActualizacion
    FROM Notas
    WHERE UsuarioId = @UsuarioId 
      AND EsArchivada = 0
      -- Este truco evalúa si pedimos una carpeta específica o las notas de la raíz (NULL)
      AND (CarpetaId = @CarpetaId OR (@CarpetaId IS NULL AND CarpetaId IS NULL))
    ORDER BY FechaActualizacion DESC;
END;
GO

-- ============================================================================
-- 5. MOVER NOTA A OTRA CARPETA (Drag & Drop)
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Notas_MoverACarpeta
    @Id UNIQUEIDENTIFIER,
    @UsuarioId UNIQUEIDENTIFIER,
    @NuevaCarpetaId UNIQUEIDENTIFIER = NULL -- NULL para moverla a la raíz
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Notas
    SET 
        CarpetaId = @NuevaCarpetaId,
        FechaActualizacion = GETUTCDATE()
    WHERE Id = @Id AND UsuarioId = @UsuarioId;
END;
GO

-- ============================================================================
-- 6. ALTERNAR FAVORITO (Estrellita)
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Notas_AlternarFavorito
    @Id UNIQUEIDENTIFIER,
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Notas
    SET 
        EsFavorita = CASE WHEN EsFavorita = 1 THEN 0 ELSE 1 END,
        FechaActualizacion = GETUTCDATE()
    WHERE Id = @Id AND UsuarioId = @UsuarioId;
END;
GO

-- ============================================================================
-- 7. ARCHIVAR NOTA (Papelera de Reciclaje)
-- En lugar de hacer un DELETE real que destruye datos, la ocultamos.
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Notas_Archivar
    @Id UNIQUEIDENTIFIER,
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Notas
    SET 
        EsArchivada = 1,
        FechaActualizacion = GETUTCDATE()
    WHERE Id = @Id AND UsuarioId = @UsuarioId;
END;
GO