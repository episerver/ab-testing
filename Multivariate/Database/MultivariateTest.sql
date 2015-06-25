--beginvalidatingquery
	if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[sp_DatabaseVersion]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    begin
            if (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = 'tblMultivariateTests'))
				select 0, 'Multivariate tables already exist'
            else 
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


---Stored Procedure Upsert tblMultivariateTests.
IF EXISTS (select * FROM SYS.OBJECTS WHERE object_id = object_id(N'[dbo].[MultivariateTest_Add]') and OBJECTPROPERTY(object_id, N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[MultivariateTest_Add]
GO

CREATE PROCEDURE [dbo].[MultivariateTest_Add]
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

	if(not exists(select * FROM dbo.tblMultivariateTests WHERE [Id] = @Id))
	begin	
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

	end
	ELSE
	begin
		update dbo.tblMultivariateTests set			
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
		where Id = @Id
	end
END 

GO


---Store Procedure Delete tblMultivariateTests.
IF EXISTS (select * FROM SYS.OBJECTS WHERE object_id = object_id(N'[dbo].[MultivariateTest_Delete]') and OBJECTPROPERTY(object_id, N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[MultivariateTest_Delete]
GO

CREATE PROCEDURE [dbo].[MultivariateTest_Delete]
	 @Id uniqueidentifier
AS
SET NOCOUNT ON
BEGIN
		Delete from dbo.tblMultivariateTests where [Id] = @Id
END 

GO


---Store Procedure Get tblMultivariateTests.
IF EXISTS (select * FROM SYS.OBJECTS WHERE object_id = object_id(N'[dbo].[MultivariateTest_Get]') and OBJECTPROPERTY(object_id, N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[MultivariateTest_Get]
GO

CREATE PROCEDURE [dbo].[MultivariateTest_Get]
	 @Id	uniqueidentifier
		
AS
SET NOCOUNT ON
BEGIN
	select * FROM dbo.tblMultivariateTests WHERE [Id] = @Id
END 

GO



---Store Procedure GetByOriginalItemId tblMultivariateTests.
IF EXISTS (select * FROM SYS.OBJECTS WHERE object_id = object_id(N'[dbo].[MultivariateTest_GetByOriginalItemId]') and OBJECTPROPERTY(object_id, N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[MultivariateTest_GetByOriginalItemId]
GO

CREATE PROCEDURE [dbo].[MultivariateTest_GetByOriginalItemId]
	@OriginalItemId uniqueidentifier	
AS
SET NOCOUNT ON
BEGIN
	select * FROM dbo.tblMultivariateTests WHERE [OriginalItemId] = @OriginalItemId
END 

GO




