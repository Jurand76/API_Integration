CREATE PROCEDURE [dbo].[sp_GetExistingPrices]
    @synchronizationId int
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.SynchronizationID, p.Code, p.PriceBuying, p.PriceDetBr, p.TaxValue, s.ShoperId 
    FROM [Prices] as p
    JOIN [ShoperIntegration] as s
    ON p.Code = s.Code
    WHERE p.SynchronizationId = @synchronizationId
END