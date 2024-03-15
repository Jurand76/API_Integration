CREATE TABLE [dbo].[Products]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[Title] NVARCHAR(255),
    [Category] NVARCHAR(128), 
    [Ean] NVARCHAR(50) NULL,
    [Isbn] NVARCHAR(50) NULL,
    [Code] NVARCHAR(50) NOT NULL UNIQUE,
    [IssueId] NVARCHAR(20) NULL,
    [Authors] NVARCHAR(1500) NULL,
    [MediaType] NVARCHAR(20) NULL,
    [Pages] NVARCHAR(8) NULL,
    [Time] NVARCHAR(16) NULL,
    [YearOfPublish] NVARCHAR(6) NULL,
    [Lectors] NVARCHAR(384) NULL,
    [ShortDescription] NVARCHAR(MAX) NULL,
    [Description] NVARCHAR(MAX) NULL,
    [ImageUrl] NVARCHAR(512) NULL,
    [TypeId] NVARCHAR(32) NULL
)
