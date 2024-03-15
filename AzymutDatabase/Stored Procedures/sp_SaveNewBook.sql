CREATE PROCEDURE dbo.sp_SaveNewBook
    @Title NVARCHAR(255),
    @Category NVARCHAR(MAX),
    @Ean NVARCHAR(50),
    @Isbn NVARCHAR(50),
    @Code NVARCHAR(50),
    @IssueId NVARCHAR(20),
    @Authors NVARCHAR(MAX),
    @MediaType NVARCHAR(20),
    @Pages NVARCHAR(8),
    @Time NVARCHAR(16),
    @YearOfPublish NVARCHAR(6),
    @Lectors NVARCHAR(MAX),
    @ShortDescription NVARCHAR(MAX),
    @Description NVARCHAR(MAX),
    @ImageUrl NVARCHAR(MAX),
    @TypeId NVARCHAR(32)
AS
BEGIN
  
 IF EXISTS (SELECT 1 FROM Products WHERE Code = @Code)
    BEGIN
       SELECT -1; -- if element exists - returns -1
    END
    ELSE
    BEGIN
        -- Insert new element to table
        INSERT INTO Products (Title, Category, Ean, Isbn, Code, IssueId, Authors, MediaType, Pages, Time, YearOfPublish, Lectors, ShortDescription, Description, ImageUrl, TypeId) 
        VALUES (@Title, @Category, @Ean, @Isbn, @Code, @IssueId, @Authors, @MediaType, @Pages, @Time, @YearOfPublish, @Lectors, @ShortDescription, @Description, @ImageUrl, @TypeId);
        
        -- Get back ID of new created element
        SELECT CAST(SCOPE_IDENTITY() as int);
    END
END