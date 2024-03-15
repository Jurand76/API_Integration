CREATE PROCEDURE sp_GetShoperIntegrationIds
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Id], [SynchronizationId], [Code], [ShoperId]
    FROM [ShoperIntegration];
END
