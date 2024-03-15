CREATE PROCEDURE [dbo].[sp_UpdateAzymutTypeId]
	@TypeId VARCHAR(16),
    @Code VARCHAR(255) 
AS
BEGIN
    -- Update statement targeting a specific row based on Code
    UPDATE [Products]
    SET TypeId = @TypeId
    WHERE Code = @Code;
END