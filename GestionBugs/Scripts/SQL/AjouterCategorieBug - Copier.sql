USE [DB_kek_Test]
GO

/****** Object:  Table [dbo].[GB_BUG]    Script Date: 30/09/2026 20:04:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[GB_BUG](
	[IdBug] [int] IDENTITY(1,1) NOT NULL,
	[Titre] [nvarchar](150) NOT NULL,
	[Application] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NOT NULL,
	[Etapes] [nvarchar](max) NULL,
	[ResultatAttendu] [nvarchar](max) NULL,
	[ResultatObtenu] [nvarchar](max) NULL,
	[MessageErreur] [nvarchar](max) NULL,
	[Priorite] [nvarchar](10) NOT NULL,
	[DateCreationUtc] [datetime2](0) NOT NULL,
	[Categorie] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_GB_BUG] PRIMARY KEY CLUSTERED 
(
	[IdBug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[GB_BUG] ADD  CONSTRAINT [DF_GB_BUG_DateCreationUtc]  DEFAULT (sysutcdatetime()) FOR [DateCreationUtc]
GO

ALTER TABLE [dbo].[GB_BUG] ADD  CONSTRAINT [DF_GB_BUG_Categorie]  DEFAULT (N'Autre') FOR [Categorie]
GO

ALTER TABLE [dbo].[GB_BUG]  WITH CHECK ADD  CONSTRAINT [CK_GB_BUG_Categorie] CHECK  (([Categorie]=N'Autre' OR [Categorie]=N'Compatibilité' OR [Categorie]=N'Données' OR [Categorie]=N'Sécurité' OR [Categorie]=N'Performance' OR [Categorie]=N'Fonctionnalité' OR [Categorie]=N'Interface utilisateur'))
GO

ALTER TABLE [dbo].[GB_BUG] CHECK CONSTRAINT [CK_GB_BUG_Categorie]
GO

ALTER TABLE [dbo].[GB_BUG]  WITH CHECK ADD  CONSTRAINT [CK_GB_BUG_Priorite] CHECK  (([Priorite]=N'Critique' OR [Priorite]=N'Haute' OR [Priorite]=N'Normale' OR [Priorite]=N'Faible'))
GO

ALTER TABLE [dbo].[GB_BUG] CHECK CONSTRAINT [CK_GB_BUG_Priorite]
GO


/****** Object:  Table [dbo].[GB_CAPTURE]    Script Date: 30/09/2026 20:06:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[GB_CAPTURE](
	[IdBug] [int] NOT NULL,
	[NomFichier] [nvarchar](255) NOT NULL,
	[MimeType] [varchar](20) NOT NULL,
	[Contenu] [varbinary](max) NOT NULL,
 CONSTRAINT [PK_GB_CAPTURE] PRIMARY KEY CLUSTERED 
(
	[IdBug] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[GB_CAPTURE]  WITH CHECK ADD  CONSTRAINT [FK_GB_CAPTURE_GB_BUG] FOREIGN KEY([IdBug])
REFERENCES [dbo].[GB_BUG] ([IdBug])
GO

ALTER TABLE [dbo].[GB_CAPTURE] CHECK CONSTRAINT [FK_GB_CAPTURE_GB_BUG]
GO

ALTER TABLE [dbo].[GB_CAPTURE]  WITH CHECK ADD  CONSTRAINT [CK_GB_CAPTURE_MimeType] CHECK  (([MimeType]='image/jpeg' OR [MimeType]='image/png'))
GO

ALTER TABLE [dbo].[GB_CAPTURE] CHECK CONSTRAINT [CK_GB_CAPTURE_MimeType]
GO

ALTER TABLE [dbo].[GB_CAPTURE]  WITH CHECK ADD  CONSTRAINT [CK_GB_CAPTURE_Taille] CHECK  ((datalength([Contenu])>=(1) AND datalength([Contenu])<=(5242880)))
GO

ALTER TABLE [dbo].[GB_CAPTURE] CHECK CONSTRAINT [CK_GB_CAPTURE_Taille]
GO


