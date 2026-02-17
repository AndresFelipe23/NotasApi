USE Anota;
GO

-- ============================================================================
-- 1. OBTENER ETIQUETAS DEL USUARIO
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Etiquetas_ObtenerPorUsuario
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, UsuarioId, Nombre, ColorHex, FechaCreacion
    FROM Etiquetas
    WHERE UsuarioId = @UsuarioId
    ORDER BY Nombre;
END;
GO

-- ============================================================================
-- 2. CREAR ETIQUETA
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Etiquetas_Crear
    @UsuarioId UNIQUEIDENTIFIER,
    @Nombre NVARCHAR(50),
    @ColorHex VARCHAR(7) = NULL,
    @NuevoId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @NuevoId = NEWID();

    INSERT INTO Etiquetas (Id, UsuarioId, Nombre, ColorHex)
    VALUES (@NuevoId, @UsuarioId, @Nombre, @ColorHex);
END;
GO

-- ============================================================================
-- 3. ACTUALIZAR ETIQUETA
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Etiquetas_Actualizar
    @Id UNIQUEIDENTIFIER,
    @UsuarioId UNIQUEIDENTIFIER,
    @Nombre NVARCHAR(50),
    @ColorHex VARCHAR(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Etiquetas
    SET Nombre = @Nombre, ColorHex = @ColorHex
    WHERE Id = @Id AND UsuarioId = @UsuarioId;
END;
GO

-- ============================================================================
-- 4. ELIMINAR ETIQUETA
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Etiquetas_Eliminar
    @Id UNIQUEIDENTIFIER,
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM Etiquetas
    WHERE Id = @Id AND UsuarioId = @UsuarioId;
END;
GO

-- ============================================================================
-- 5. OBTENER ETIQUETAS DE UNA NOTA
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_NotasEtiquetas_ObtenerPorNota
    @NotaId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT e.Id, e.UsuarioId, e.Nombre, e.ColorHex, e.FechaCreacion
    FROM Etiquetas e
    INNER JOIN NotasEtiquetas ne ON ne.EtiquetaId = e.Id
    WHERE ne.NotaId = @NotaId
    ORDER BY e.Nombre;
END;
GO

-- ============================================================================
-- 6. ASIGNAR ETIQUETAS A UNA NOTA (reemplaza las actuales)
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_NotasEtiquetas_Asignar
    @NotaId UNIQUEIDENTIFIER,
    @EtiquetaIds NVARCHAR(MAX) -- JSON array de GUIDs: ["guid1","guid2"]
AS
BEGIN
    SET NOCOUNT ON;

    -- Eliminar asignaciones actuales
    DELETE FROM NotasEtiquetas WHERE NotaId = @NotaId;

    -- Insertar nuevas (solo si hay IDs)
    IF LEN(LTRIM(RTRIM(@EtiquetaIds))) > 2
    BEGIN
        INSERT INTO NotasEtiquetas (NotaId, EtiquetaId)
        SELECT @NotaId, TRY_CAST([value] AS UNIQUEIDENTIFIER)
        FROM OPENJSON(@EtiquetaIds)
        WHERE TRY_CAST([value] AS UNIQUEIDENTIFIER) IS NOT NULL;
    END
END;
GO
