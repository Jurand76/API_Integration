CREATE PROCEDURE dbo.sp_SaveNewOrder
    @Id INT,
    @SynchronizationId INT,
    @OrderId INT,
    @Date DATETIME,
    @IssueId NVARCHAR(32),
    @Code NVARCHAR(50),
    @TypeId NVARCHAR(32),
    @Mail NVARCHAR(256),
    @Status INT
AS
BEGIN
    -- Insert new element to table
    INSERT INTO Orders (SynchronizationId, OrderId, Date, IssueId, Code, TypeId, Mail, Status) 
    VALUES (@SynchronizationId, @OrderId, @Date, @IssueId, @Code, @TypeId, @Mail, @Status);
        
    -- Get back ID of new created element
    SELECT CAST(SCOPE_IDENTITY() as int);    
END
