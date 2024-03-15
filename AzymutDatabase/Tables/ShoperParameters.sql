CREATE TABLE [dbo].[ShoperParameters]
(
	[Id] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[SynchronizationId] INT NOT NULL,
	[MainCategoryId] INT NOT NULL,
	[MainProducerId] INT NOT NULL,
	[OrderStatus1] INT NOT NULL,
	[OrderStatus2] INT NOT NULL,
	[OrderStatus3] INT NOT NULL,
	[OrderStatus4] INT NOT NULL
)
