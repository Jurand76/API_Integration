CREATE PROCEDURE dbo.sp_GetOrdersWithStatus
   @synchronizationId int,
   @status int
AS
BEGIN
    -- Get new created orders from Orders table
    SELECT
        [Id],
        [SynchronizationId],
        [OrderId],
        [Date],
        [IssueId],
        [Code],
        [TypeId],
        [Mail],
        [Status]
    FROM
        Orders
    WHERE [Status] = @status AND [SynchronizationId] = @synchronizationId
END
