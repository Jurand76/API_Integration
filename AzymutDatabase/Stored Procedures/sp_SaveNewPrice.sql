CREATE PROCEDURE [dbo].[sp_SaveNewPrice]
    @SynchronizationId INT,
    @Code VARCHAR(50),
    @PriceBuying DECIMAL(18,2),
    @PriceDetBr DECIMAL(18,2),
    @TaxValue DECIMAL(18,2)
AS

BEGIN
    IF EXISTS (SELECT 1 FROM Prices WHERE Code = @Code AND SynchronizationId = @SynchronizationId)
        BEGIN
            SELECT -1; -- if element exists - returns -1
        END
    ELSE
        BEGIN
            -- Insert new element to table
            INSERT INTO Prices (SynchronizationId, Code, PriceBuying, PriceDetBr, TaxValue) 
            VALUES (@SynchronizationId, @Code, @PriceBuying, @PriceDetBr, @TaxValue);
        
            -- Get back ID of new created element
            SELECT CAST(SCOPE_IDENTITY() as int);
        END
END
