CREATE PROCEDURE dbo.sp_GetLastSyncDate
AS
BEGIN
    SELECT TOP 1 [Date], [ItemsChanged]
    FROM [dbo].[DatabaseSync]
    ORDER BY [Date] DESC;
END;