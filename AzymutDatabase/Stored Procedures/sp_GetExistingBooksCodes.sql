CREATE PROCEDURE [dbo].[sp_GetExistingBooksCodes]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Code]
    FROM [Products];
END
