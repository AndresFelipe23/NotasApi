USE Anota;
GO

-- ============================================================================
-- 1. OBTENER USUARIO POR ID (Para autenticación y perfiles)
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Usuarios_ObtenerPorId
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id, Nombre, Apellido, Correo, PasswordHash, FotoPerfilUrl, 
        EsActivo, FechaUltimoAcceso, FechaCreacion, FechaActualizacion
    FROM Usuarios 
    WHERE Id = @Id;
END;
GO

-- ============================================================================
-- 2. OBTENER USUARIO POR CORREO (Para login - debe ser rápido)
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Usuarios_ObtenerPorCorreo
    @Correo NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id, Nombre, Apellido, Correo, PasswordHash, FotoPerfilUrl, 
        EsActivo, FechaUltimoAcceso, FechaCreacion, FechaActualizacion
    FROM Usuarios 
    WHERE Correo = @Correo;
END;
GO

-- ============================================================================
-- 3. CREAR USUARIO NUEVO
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Usuarios_Crear
    @Id UNIQUEIDENTIFIER,
    @Nombre NVARCHAR(100),
    @Apellido NVARCHAR(100) = NULL,
    @Correo NVARCHAR(150),
    @PasswordHash NVARCHAR(MAX),
    @FotoPerfilUrl NVARCHAR(500) = NULL,
    @EsActivo BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Usuarios (
        Id, Nombre, Apellido, Correo, PasswordHash, FotoPerfilUrl, 
        EsActivo, FechaCreacion, FechaActualizacion
    )
    VALUES (
        @Id, @Nombre, @Apellido, @Correo, @PasswordHash, @FotoPerfilUrl, 
        @EsActivo, GETUTCDATE(), GETUTCDATE()
    );
END;
GO

-- ============================================================================
-- 4. ACTUALIZAR USUARIO
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Usuarios_Actualizar
    @Id UNIQUEIDENTIFIER,
    @Nombre NVARCHAR(100),
    @Apellido NVARCHAR(100) = NULL,
    @Correo NVARCHAR(150),
    @PasswordHash NVARCHAR(MAX),
    @FotoPerfilUrl NVARCHAR(500) = NULL,
    @EsActivo BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Usuarios 
    SET 
        Nombre = @Nombre, 
        Apellido = @Apellido, 
        Correo = @Correo, 
        PasswordHash = @PasswordHash, 
        FotoPerfilUrl = @FotoPerfilUrl, 
        EsActivo = @EsActivo, 
        FechaActualizacion = GETUTCDATE()
    WHERE Id = @Id;
END;
GO

-- ============================================================================
-- 5. ACTUALIZAR ÚLTIMO ACCESO (Llamado en cada login - debe ser ultra rápido)
-- ============================================================================
CREATE OR ALTER PROCEDURE usp_Usuarios_ActualizarUltimoAcceso
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Usuarios 
    SET FechaUltimoAcceso = GETUTCDATE()
    WHERE Id = @Id;
END;
GO
