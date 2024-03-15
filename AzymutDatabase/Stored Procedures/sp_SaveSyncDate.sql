CREATE PROCEDURE [dbo].[sp_SaveSyncDate]
	@Date datetime,
	@ItemsChanged int
AS
BEGIN
	INSERT INTO [DatabaseSync] (Date, ItemsChanged)
	VALUES (@Date, @ItemsChanged)
END
