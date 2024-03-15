CREATE TABLE [dbo].[Prices]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[SynchronizationId] INT NOT NULL,
	[Code] NVARCHAR(50) NOT NULL,
	[PriceBuying] DECIMAL (18,2) NULL,
	[PriceDetBr] DECIMAL (18,2) NULL,
	[TaxValue] DECIMAL (18,2) NULL,
	FOREIGN KEY ([Code]) REFERENCES Products([Code])
)
