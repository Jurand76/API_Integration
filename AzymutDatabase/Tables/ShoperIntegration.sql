CREATE TABLE [dbo].[ShoperIntegration]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[SynchronizationId] INT NOT NULL,
	[Code] NVARCHAR(50) NOT NULL,
	[ShoperId] INT NOT NULL,
	FOREIGN KEY ([Code]) REFERENCES Products([Code])
)