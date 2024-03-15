CREATE PROCEDURE [dbo].[sp_GetExistingBooksCodesIssuesAndTypes]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT [Code], [TypeId], [IssueId]
    FROM [Products];
END