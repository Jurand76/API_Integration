CREATE PROCEDURE [dbo].[sp_GetProductsNotInShoperIntegration]
    @SynchronizationId INT
AS
BEGIN
    SELECT 
        p.Title,
        p.Category,
        p.Ean,
        p.Isbn,
        p.Code,
        p.IssueId,
        p.Authors,
        p.MediaType,
        p.TypeId,
        p.Pages,
        p.Time,
        p.YearOfPublish,
        p.Lectors,
        p.ShortDescription,
        p.Description,
        p.ImageUrl,
        pr.PriceBuying,
        pr.PriceDetBr,
        pr.TaxValue
    FROM [dbo].[Products] p
    LEFT JOIN [dbo].[ShoperIntegration] si ON p.Code = si.Code AND si.SynchronizationId = @SynchronizationId
    LEFT JOIN [dbo].[Prices] pr ON p.Code = pr.Code AND pr.SynchronizationId = @SynchronizationId
    WHERE si.Code IS NULL AND pr.PriceBuying > 0;
END
