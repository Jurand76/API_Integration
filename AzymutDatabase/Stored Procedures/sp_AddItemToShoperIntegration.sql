CREATE PROCEDURE [dbo].[sp_AddItemToShoperIntegration]
    @SynchronizationId INT,
    @Code NVARCHAR(50),
    @ShoperId INT
AS
BEGIN
    INSERT INTO [dbo].[ShoperIntegration] (SynchronizationId, Code, ShoperId)
    VALUES (@SynchronizationId, @Code, @ShoperId);
END