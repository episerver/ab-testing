--beginvalidatingquery
	if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[sp_DatabaseVersion]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    begin
		 select 1, 'Upgrading database'
    end
    else
            select -1, 'Not an EPiServer database'
--endvalidatingquery

-- Create tblMultivariateTests Table to Store MultivariateTest Information.
IF NOT (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = 'tblMultivariateTests'))
BEGIN

    CREATE TABLE [dbo].[tblMultivariateTests](
               [Id] [uniqueidentifier] NOT NULL,
			   [Title] [nvarchar](255) NOT NULL,
			   [Owner] [nvarchar](100) NOT NULL,
               [OriginalItemId] [uniqueidentifier] NOT NULL,
               [VariantItemId] [uniqueidentifier] NOT NULL,
			   [ConversionItemId] [uniqueidentifier] NOT NULL,  
			   [State] [nvarchar](10) NOT NULL, 
			   [StartDate] [datetime] NOT NULL,
		       [EndDate] [datetime], 
		       [LastModifiedDate] [datetime],    
		       [LastModifiedBy] [nvarchar](100)             
				CONSTRAINT [PK_tblMultivariateTests] PRIMARY KEY CLUSTERED 
				(
							   [Id] ASC
				)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
				) ON [PRIMARY]
				
END

GO

-- Create tblMultivariateTestsResults Table to Store results of the multivariatetests i.e. views and conversions.
IF NOT (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = 'tblMultivariateTestsResults'))
BEGIN

    CREATE TABLE [dbo].[tblMultivariateTestsResults](
               [Id] [uniqueidentifier] NOT NULL,
			   [TestId] [uniqueidentifier] NOT NULL,
               [ItemId] [uniqueidentifier] NOT NULL,
		       [Views] [int],
			   [Conversions][int]        
				CONSTRAINT [PK_tblMultivariateTestsResults] PRIMARY KEY CLUSTERED 
				(
							   [Id] ASC
				)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
				CONSTRAINT [FK_tblMultivariateTestsResults] FOREIGN KEY ([TestId])
				 REFERENCES [tblMultivariateTests] ([Id]) 
				) ON [PRIMARY]			
END

GO


---Stored Procedure Upsert tblMultivariateTests.
IF EXISTS (SELECT * FROM SYS.OBJECTS WHERE object_id = object_id(N'[dbo].[MultivariateTest_Save]') and OBJECTPROPERTY(object_id, N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[MultivariateTest_Save]
GO

CREATE PROCEDURE [dbo].[MultivariateTest_Save]
	 @Id uniqueidentifier
	,@Title nvarchar(255)
    ,@Owner nvarchar(100)
	,@OriginalItemId uniqueidentifier
	,@VariantItemId uniqueidentifier
	,@ConversionItemId uniqueidentifier
	,@State nvarchar(100)
	,@StartDate datetime
    ,@EndDate datetime 
    ,@LastModifiedDate datetime   
    ,@LastModifiedBy nvarchar(100)
AS
SET NOCOUNT ON
BEGIN

	if(not exists(SELECT * FROM dbo.tblMultivariateTests WHERE [Id] = @Id))
	BEGIN	
		INSERT INTO dbo.tblMultivariateTests(
			 [Id]
	        ,[Title]
	        ,[Owner]
            ,[OriginalItemId]
            ,[VariantItemId]
			,[ConversionItemId] 
			,[State]
			,[StartDate]
		    ,[EndDate] 
		    ,[LastModifiedDate]   
		    ,[LastModifiedBy]
		)
		VALUES(
		    @Id
		    ,@Title
	        ,@Owner
            ,@OriginalItemId
            ,@VariantItemId
			,@ConversionItemId
			,@State
			,@StartDate
		    ,@EndDate 
		    ,@LastModifiedDate   
		    ,@LastModifiedBy
		)

	END
	ELSE
	BEGIN
		UPDATE dbo.tblMultivariateTests SET			
	        [Title] = @Title
	        ,[Owner] = @Owner
            ,[OriginalItemId] = @OriginalItemId
            ,[VariantItemId] = @VariantItemId
			,[ConversionItemId] = @ConversionItemId
			,[State] = @State
			,[StartDate] = @StartDate
		    ,[EndDate] = @EndDate 
		    ,[LastModifiedDate] = @LastModifiedDate   
		    ,[LastModifiedBy] = @LastModifiedBy 
		WHERE Id = @Id
	END
END 

GO


---Store Procedure Delete tblMultivariateTests.
IF EXISTS (SELECT * FROM SYS.OBJECTS WHERE object_id = object_id(N'[dbo].[MultivariateTest_Delete]') and OBJECTPROPERTY(object_id, N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[MultivariateTest_Delete]
GO

CREATE PROCEDURE [dbo].[MultivariateTest_Delete]
	 @Id uniqueidentifier
AS
SET NOCOUNT ON
BEGIN
        DELETE FROM dbo.tblMultivariateTestsResults WHERE [TestId] = @Id
		DELETE FROM dbo.tblMultivariateTests WHERE [Id] = @Id
END 

GO


---Store Procedure Get tblMultivariateTests.
IF EXISTS (SELECT * FROM SYS.OBJECTS WHERE object_id = object_id(N'[dbo].[MultivariateTest_GetTest]') and OBJECTPROPERTY(object_id, N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[MultivariateTest_GetTest]
GO

CREATE PROCEDURE [dbo].[MultivariateTest_GetTest]
	 @Id	uniqueidentifier
		
AS
SET NOCOUNT ON
BEGIN
	SELECT * FROM dbo.tblMultivariateTests WHERE [Id] = @Id
END 

GO


---Store Procedure GetTestResults from tblMultivariateTestsResults.
IF EXISTS (SELECT * FROM SYS.OBJECTS WHERE object_id = object_id(N'[dbo].[MultivariateTest_GetTestResults]') and OBJECTPROPERTY(object_id, N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[MultivariateTest_GetTestResults]
GO

CREATE PROCEDURE [dbo].[MultivariateTest_GetTestResults]
	 @Id	uniqueidentifier
		
AS
SET NOCOUNT ON
BEGIN
	SELECT * FROM  dbo.tblMultivariateTestsResults WHERE [TestId] = @Id
END 

GO


---Store Procedure GetByOriginalItemId tblMultivariateTests.
IF EXISTS (SELECT * FROM SYS.OBJECTS WHERE object_id = object_id(N'[dbo].[MultivariateTest_GetByOriginalItemId]') and OBJECTPROPERTY(object_id, N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[MultivariateTest_GetByOriginalItemId]
GO

CREATE PROCEDURE [dbo].[MultivariateTest_GetByOriginalItemId]
	@OriginalItemId uniqueidentifier	
AS
SET NOCOUNT ON
BEGIN
	SELECT * FROM dbo.tblMultivariateTests WHERE [OriginalItemId] = @OriginalItemId
END 

GO

---Stored Procedure Increment Views.
IF EXISTS (select * FROM SYS.OBJECTS WHERE object_id = object_id(N'[dbo].[MultivariateTest_IncrementViews]') and OBJECTPROPERTY(object_id, N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[MultivariateTest_IncrementViews]
GO

CREATE PROCEDURE [dbo].[MultivariateTest_IncrementViews]
	 @TestId uniqueidentifier
	,@ItemId uniqueidentifier
AS
SET NOCOUNT ON
BEGIN

	if(not exists(SELECT * FROM dbo.tblMultivariateTestsResults WHERE [TestId] = @TestId))
	BEGIN	
		INSERT INTO dbo.tblMultivariateTestsResults(
			 [Id]
	        ,[TestId]
	        ,[ItemId]
            ,[Views]
            ,[Conversions]
		)
		VALUES(
		    NEWID()
		    ,@TestId
	        ,@ItemId
            ,1
            ,0
		)

	END
	ELSE
	BEGIN
	DECLARE @existingViews int
	SELECT @existingViews = [Views] FROM dbo.tblMultivariateTestsResults WHERE [TestId] = @TestId and [ItemId] = @ItemId
		UPDATE dbo.tblMultivariateTestsResults 
		SET	[Views] = @existingViews + 1
		WHERE [TestId] = @TestId and [ItemId] = @ItemId
	END
END 

GO

---Stored Procedure Increment Conversions.
IF EXISTS (select * FROM SYS.OBJECTS WHERE object_id = object_id(N'[dbo].[MultivariateTest_IncrementConversions]') and OBJECTPROPERTY(object_id, N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[MultivariateTest_IncrementConversions]
GO

CREATE PROCEDURE [dbo].[MultivariateTest_IncrementConversions]
	 @TestId uniqueidentifier
	,@ItemId uniqueidentifier
AS
SET NOCOUNT ON
BEGIN
	DECLARE @existingConversions int
	SELECT @existingConversions = [Conversions] FROM dbo.tblMultivariateTestsResults WHERE [TestId] = @TestId and [ItemId] = @ItemId
		UPDATE dbo.tblMultivariateTestsResults 
		SET [Conversions] = @existingConversions + 1
		WHERE [TestId] = @TestId and [ItemId] = @ItemId
END 

GO
