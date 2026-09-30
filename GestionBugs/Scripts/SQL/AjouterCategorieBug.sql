/*
    Migration à exécuter une seule fois dans DB_kek_Test.
    Elle ajoute une catégorie aux bugs existants (valeur initiale : Autre)
    et met à jour les procédures utilisées par l'application.
    Ce fichier n'est pas exécuté par l'application.
*/
USE [DB_kek_Test];
GO

ALTER TABLE dbo.GB_BUG
ADD Categorie nvarchar(30) NOT NULL
    CONSTRAINT DF_GB_BUG_Categorie DEFAULT (N'Autre');
GO

ALTER TABLE dbo.GB_BUG
ADD CONSTRAINT CK_GB_BUG_Categorie
CHECK (Categorie IN
(
    N'Interface utilisateur',
    N'Fonctionnalité',
    N'Performance',
    N'Sécurité',
    N'Données',
    N'Compatibilité',
    N'Autre'
));
GO

CREATE OR ALTER PROCEDURE dbo.GB_P_CREER_BUG
    @Titre              nvarchar(150),
    @Application        nvarchar(max),
    @Categorie          nvarchar(30),
    @Description        nvarchar(max),
    @Etapes             nvarchar(max) = NULL,
    @ResultatAttendu    nvarchar(max) = NULL,
    @ResultatObtenu     nvarchar(max) = NULL,
    @MessageErreur      nvarchar(max) = NULL,
    @Priorite           nvarchar(10),
    @IdBug              int OUTPUT,
    @CaptureContenu     varbinary(max) = NULL,
    @CaptureMime        varchar(20) = NULL,
    @CaptureNomFichier  nvarchar(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @Categorie IS NULL OR @Categorie NOT IN
    (
        N'Interface utilisateur',
        N'Fonctionnalité',
        N'Performance',
        N'Sécurité',
        N'Données',
        N'Compatibilité',
        N'Autre'
    )
    BEGIN
        THROW 50003, 'La catégorie du bug est invalide.', 1;
    END;

    IF @CaptureContenu IS NULL
       AND (@CaptureMime IS NOT NULL OR @CaptureNomFichier IS NOT NULL)
    BEGIN
        THROW 50001, 'Les informations de capture nécessitent un contenu image.', 1;
    END;

    IF @CaptureContenu IS NOT NULL
       AND
       (
           DATALENGTH(@CaptureContenu) NOT BETWEEN 1 AND 5242880
           OR @CaptureMime NOT IN ('image/png', 'image/jpeg')
           OR @CaptureNomFichier IS NULL
       )
    BEGIN
        THROW 50002, 'La capture doit être un PNG ou JPEG de 5 Mo maximum.', 1;
    END;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.GB_BUG
        (
            Titre,
            Application,
            Categorie,
            Description,
            Etapes,
            ResultatAttendu,
            ResultatObtenu,
            MessageErreur,
            Priorite
        )
        VALUES
        (
            @Titre,
            @Application,
            @Categorie,
            @Description,
            @Etapes,
            @ResultatAttendu,
            @ResultatObtenu,
            @MessageErreur,
            @Priorite
        );

        SET @IdBug = CONVERT(int, SCOPE_IDENTITY());

        IF @CaptureContenu IS NOT NULL
        BEGIN
            INSERT INTO dbo.GB_CAPTURE (IdBug, NomFichier, MimeType, Contenu)
            VALUES (@IdBug, @CaptureNomFichier, @CaptureMime, @CaptureContenu);
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE dbo.GB_P_CHARGER_BUG
    @IdBug int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        IdBug,
        Titre,
        Application,
        Categorie,
        Description,
        Etapes,
        ResultatAttendu,
        ResultatObtenu,
        MessageErreur,
        Priorite,
        DateCreationUtc
    FROM dbo.GB_BUG
    WHERE IdBug = @IdBug;
END;
GO

