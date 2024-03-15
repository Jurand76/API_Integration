CREATE PROCEDURE [dbo].[sp_CheckOrderExistence]
	@OrderId INT,
    @Code NVARCHAR(50),
    @SynchronizationId INT
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM [dbo].[Orders]
        WHERE [OrderId] = @OrderId AND [Code] = @Code AND [SynchronizationId] = @SynchronizationId
    )
    BEGIN
        -- If exists - return 1 as a result set
        SELECT 1 AS ExistStatus;
    END
    ELSE
    BEGIN
        -- If not exists - return 0 as a result set
        SELECT 0 AS ExistStatus;
    END
END;
