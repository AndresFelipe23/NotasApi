USE Anota;
GO

-- ============================================================================
-- 1. CREAR CARPETA RAÍZ
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Carpetas_CrearRaiz
    @UsuarioId UNIQUEIDENTIFIER,
    @Nombre NVARCHAR(100),
    @Icono NVARCHAR(50) = NULL,
    @ColorHex VARCHAR(7) = NULL,
    @NuevoId UNIQUEIDENTIFIER OUTPUT 
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UltimoNodoRaiz HIERARCHYID;
    DECLARE @NuevoNodoRaiz HIERARCHYID;
    
    SELECT @UltimoNodoRaiz = MAX(RutaJerarquica)
    FROM Carpetas
    WHERE UsuarioId = @UsuarioId AND RutaJerarquica.GetLevel() = 1;

    SET @NuevoNodoRaiz = hierarchyid::GetRoot().GetDescendant(@UltimoNodoRaiz, NULL);
    SET @NuevoId = NEWID();

    INSERT INTO Carpetas (Id, UsuarioId, Nombre, RutaJerarquica, Icono, ColorHex)
    VALUES (@NuevoId, @UsuarioId, @Nombre, @NuevoNodoRaiz, @Icono, @ColorHex);
END;
GO

-- ============================================================================
-- 2. CREAR SUBCARPETA
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Carpetas_CrearSubcarpeta
    @UsuarioId UNIQUEIDENTIFIER,
    @CarpetaPadreId UNIQUEIDENTIFIER,
    @Nombre NVARCHAR(100),
    @Icono NVARCHAR(50) = NULL,
    @ColorHex VARCHAR(7) = NULL,
    @NuevoId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RutaPadre HIERARCHYID;
    DECLARE @UltimoHijo HIERARCHYID;
    DECLARE @NuevaRutaHija HIERARCHYID;

    SELECT @RutaPadre = RutaJerarquica 
    FROM Carpetas 
    WHERE Id = @CarpetaPadreId AND UsuarioId = @UsuarioId;

    IF @RutaPadre IS NULL
    BEGIN
        -- ¡Aquí está la magia del punto y coma para evitar el error!
        ;THROW 50001, 'La carpeta padre no existe o no pertenece al usuario.', 1;
    END

    SELECT @UltimoHijo = MAX(RutaJerarquica)
    FROM Carpetas
    WHERE UsuarioId = @UsuarioId AND RutaJerarquica.GetAncestor(1) = @RutaPadre;

    SET @NuevaRutaHija = @RutaPadre.GetDescendant(@UltimoHijo, NULL);
    SET @NuevoId = NEWID();

    INSERT INTO Carpetas (Id, UsuarioId, Nombre, RutaJerarquica, Icono, ColorHex)
    VALUES (@NuevoId, @UsuarioId, @Nombre, @NuevaRutaHija, @Icono, @ColorHex);
END;
GO

-- ============================================================================
-- 3. OBTENER EL ÁRBOL DE CARPETAS
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Carpetas_ObtenerArbol
    @UsuarioId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id,
        Nombre,
        Icono,
        ColorHex,
        Nivel,
        RutaJerarquica.ToString() AS RutaString,
        Orden
    FROM Carpetas
    WHERE UsuarioId = @UsuarioId
    ORDER BY RutaJerarquica; 
END;
GO

-- ============================================================================
-- 4. ELIMINAR CARPETA Y SUS DESCENDIENTES
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Carpetas_EliminarRama
    @UsuarioId UNIQUEIDENTIFIER,
    @CarpetaId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RutaABorrar HIERARCHYID;

    SELECT @RutaABorrar = RutaJerarquica 
    FROM Carpetas 
    WHERE Id = @CarpetaId AND UsuarioId = @UsuarioId;

    IF @RutaABorrar IS NULL RETURN; 

    BEGIN TRANSACTION;
    BEGIN TRY
        
        UPDATE Notas
        SET CarpetaId = NULL
        FROM Notas N
        INNER JOIN Carpetas C ON N.CarpetaId = C.Id
        WHERE C.UsuarioId = @UsuarioId 
          AND C.RutaJerarquica.IsDescendantOf(@RutaABorrar) = 1;

        DELETE FROM Carpetas
        WHERE UsuarioId = @UsuarioId 
          AND RutaJerarquica.IsDescendantOf(@RutaABorrar) = 1;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        -- Validamos que haya una transacción activa antes de hacer rollback
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRANSACTION;
        END
        
        -- El punto y coma salvador de nuevo
        ;THROW;
    END CATCH
END;
GO