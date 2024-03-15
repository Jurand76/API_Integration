CREATE TABLE [dbo].[Orders]
(
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [SynchronizationId] INT NOT NULL,
    [OrderId] INT NOT NULL,
    [Date] DATETIME NOT NULL,
    [IssueId] NVARCHAR(32) NOT NULL,
    [Code] NVARCHAR(50) NOT NULL,
    [TypeId] NVARCHAR(32) NOT NULL,
    [Mail] NVARCHAR(256) NOT NULL,
    [Status] INT NOT NULL,
);
