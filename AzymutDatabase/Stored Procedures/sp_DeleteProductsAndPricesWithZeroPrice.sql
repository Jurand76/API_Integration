CREATE PROCEDURE [dbo].[sp_DeleteProductsAndPricesWithZeroPrice]
AS
BEGIN
    BEGIN TRANSACTION;

    BEGIN TRY
        -- Delete records with PriceBuying = 0
        DELETE FROM [Azymut].[dbo].[Prices]
        WHERE PriceBuying = 0;

        -- Delete records from Products, which have the same Code as deleted from Prices
        DELETE FROM [Azymut].[dbo].[Products]
        WHERE [Code] IN (SELECT [Code] FROM [Azymut].[dbo].[Prices] WHERE PriceBuying = 0);

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;
    END CATCH;
END;
