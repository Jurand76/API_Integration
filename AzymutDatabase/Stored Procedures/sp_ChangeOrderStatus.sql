CREATE PROCEDURE [dbo].[sp_ChangeOrderStatus]
    @synchronizationId INT,
    @orderId INT,
    @code NVARCHAR(50),
    @newStatus INT
AS
BEGIN
    UPDATE [dbo].[Orders]
    SET [Status] = @newStatus
    WHERE [SynchronizationId] = @synchronizationId
      AND [OrderId] = @orderId
      AND [Code] = @code;
END;