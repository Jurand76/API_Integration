CREATE PROCEDURE [dbo].[sp_GetShoperParameters]
	@synchronizationId int
AS
BEGIN
	SELECT SynchronizationId, MainCategoryId, MainProducerId, OrderStatus1, OrderStatus2, OrderStatus3, OrderStatus4 	
	FROM ShoperParameters
	WHERE SynchronizationId = @synchronizationId
END		
