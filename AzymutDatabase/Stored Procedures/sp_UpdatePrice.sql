CREATE PROCEDURE [dbo].[sp_UpdatePrice]
    @Code VARCHAR(50),
    @SynchronizationId INT,
    @PriceBuying DECIMAL(18, 2),
    @PriceDetBr DECIMAL(18, 2),
    @TaxValue DECIMAL(18, 2),
    @ShoperId INT
AS
BEGIN
    UPDATE [Prices]
    SET PriceBuying = @PriceBuying,
        PriceDetBr = @PriceDetBr
    WHERE Code = @Code
      AND SynchronizationId = @SynchronizationId;
END;