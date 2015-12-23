--beginvalidatingquery
	if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[sp_DatabaseVersion]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
    begin
		 select 1, 'Upgrading database'
    end
    else
		select -1, 'Not an EPiServer database'
--endvalidatingquery

-- Create MultivariateTest Tables to Store MultivariateTest Information.
DECLARE @CurrentMigration [nvarchar](max)

IF object_id('[dbo].[__MigrationHistory]') IS NOT NULL
    SELECT @CurrentMigration =
        (SELECT TOP (1) 
        [Project1].[MigrationId] AS [MigrationId]
        FROM ( SELECT 
        [Extent1].[MigrationId] AS [MigrationId]
        FROM [dbo].[__MigrationHistory] AS [Extent1]
        WHERE [Extent1].[ContextKey] = N'EPiServer.Marketing.Multivariate.Dal.Migrations.Configuration'
        )  AS [Project1]
        ORDER BY [Project1].[MigrationId] DESC)

IF @CurrentMigration IS NULL
    SET @CurrentMigration = '0'

IF @CurrentMigration < '201512151848336_Initial'
BEGIN
    CREATE TABLE [dbo].[tblConversion] (
        [Id] [uniqueidentifier] NOT NULL DEFAULT newsequentialid(),
        [TestId] [uniqueidentifier] NOT NULL,
        [ConversionString] [nvarchar](max) NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [ModifiedDate] [datetime] NOT NULL,
        CONSTRAINT [PK_dbo.tblConversion] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_TestId] ON [dbo].[tblConversion]([TestId])
    CREATE TABLE [dbo].[tblMultivariateTest] (
        [Id] [uniqueidentifier] NOT NULL DEFAULT newsequentialid(),
        [Title] [nvarchar](255) NOT NULL,
        [Owner] [nvarchar](100) NOT NULL,
        [OriginalItemId] [uniqueidentifier] NOT NULL,
        [TestState] [int],
        [StartDate] [datetime] NOT NULL,
        [EndDate] [datetime] NOT NULL,
        [LastModifiedDate] [datetime],
        [LastModifiedBy] [nvarchar](100),
        [CreatedDate] [datetime] NOT NULL,
        [ModifiedDate] [datetime] NOT NULL,
        CONSTRAINT [PK_dbo.tblMultivariateTest] PRIMARY KEY ([Id])
    )
    CREATE TABLE [dbo].[tblKeyPerformanceIndicator] (
        [Id] [uniqueidentifier] NOT NULL DEFAULT newsequentialid(),
        [TestId] [uniqueidentifier] NOT NULL,
        [KeyPerformanceIndicatorId] [uniqueidentifier],
        [CreatedDate] [datetime] NOT NULL,
        [ModifiedDate] [datetime] NOT NULL,
        CONSTRAINT [PK_dbo.tblKeyPerformanceIndicator] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_TestId] ON [dbo].[tblKeyPerformanceIndicator]([TestId])
    CREATE TABLE [dbo].[tblMultivariateTestResult] (
        [Id] [uniqueidentifier] NOT NULL DEFAULT newsequentialid(),
        [TestId] [uniqueidentifier] NOT NULL,
        [ItemId] [uniqueidentifier] NOT NULL,
        [Views] [int] NOT NULL,
        [Conversions] [int] NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [ModifiedDate] [datetime] NOT NULL,
        CONSTRAINT [PK_dbo.tblMultivariateTestResult] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_TestId] ON [dbo].[tblMultivariateTestResult]([TestId])
    CREATE TABLE [dbo].[tblVariant] (
        [Id] [uniqueidentifier] NOT NULL DEFAULT newsequentialid(),
        [TestId] [uniqueidentifier] NOT NULL,
        [VariantId] [uniqueidentifier] NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [ModifiedDate] [datetime] NOT NULL,
        CONSTRAINT [PK_dbo.tblVariant] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_TestId] ON [dbo].[tblVariant]([TestId])
    ALTER TABLE [dbo].[tblConversion] ADD CONSTRAINT [FK_dbo.tblConversion_dbo.tblMultivariateTest_TestId] FOREIGN KEY ([TestId]) REFERENCES [dbo].[tblMultivariateTest] ([Id]) ON DELETE CASCADE
    ALTER TABLE [dbo].[tblKeyPerformanceIndicator] ADD CONSTRAINT [FK_dbo.tblKeyPerformanceIndicator_dbo.tblMultivariateTest_TestId] FOREIGN KEY ([TestId]) REFERENCES [dbo].[tblMultivariateTest] ([Id]) ON DELETE CASCADE
    ALTER TABLE [dbo].[tblMultivariateTestResult] ADD CONSTRAINT [FK_dbo.tblMultivariateTestResult_dbo.tblMultivariateTest_TestId] FOREIGN KEY ([TestId]) REFERENCES [dbo].[tblMultivariateTest] ([Id]) ON DELETE CASCADE
    ALTER TABLE [dbo].[tblVariant] ADD CONSTRAINT [FK_dbo.tblVariant_dbo.tblMultivariateTest_TestId] FOREIGN KEY ([TestId]) REFERENCES [dbo].[tblMultivariateTest] ([Id]) ON DELETE CASCADE
    CREATE TABLE [dbo].[__MigrationHistory] (
        [MigrationId] [nvarchar](150) NOT NULL,
        [ContextKey] [nvarchar](300) NOT NULL,
        [Model] [varbinary](max) NOT NULL,
        [ProductVersion] [nvarchar](32) NOT NULL,
        CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY ([MigrationId], [ContextKey])
    )
    INSERT [dbo].[__MigrationHistory]([MigrationId], [ContextKey], [Model], [ProductVersion])
    VALUES (N'201512151848336_Initial', N'EPiServer.Marketing.Multivariate.Dal.Migrations.Configuration',  0x1F8B0800000000000400ED5CDB6EDC36107D2FD07F10F4D416CECA7610A035761338BE1446E30BBCB6D1B78096B86B2212B51129C746D12FEB433FA9BFD0D1EA4E8A12A5D5DA5AC70810D81279C8191E0E87A319FFF7CFBFE30F0F9E6BDCE380119F4ECC9DD1B669606AFB0EA1F38919F2D99B5FCD0FEF7FFC617CE4780FC64DDAEE6DD40E7A523631EF385FEC5916B3EFB087D8C82376E0337FC647B6EF59C8F1ADDDEDEDDFAC9D1D0B038409588631BE0C29271E5EFE02BF1EF8D4C60B1E22F7D477B0CB92E7F066BA4435CE9087D902D978621E5D90290E60C6A353147CC11C263A3A0D5D4EEE514010C7A343E49AC6BE4B10CC6D8ADD9969204A7D8E38CC7CEF9AE1290F7C3A9F2EE00172AF1E1718DACD90CB7022D15EDE5C57B8EDDD48382BEF9842D921E3BED71270E76DA22D4BECDE49E766A64DD0E711E89D3F46522F753A3141F5C9EA9B8638DCDE811B444D3594BE5CB7510EB665E875D9CA3805D48BFE6D1907D0280CF084E29007085A5C84B72EB1FFC08F57FE174C273474DDA24C2015BC2B3D80471781BFC0017FBCC4B344D213C734AC723F4BEC98752BF48935F07B48E0E733181BDDBA38634C415953EE07F8774C7100D2391788731CD008022F752E0D2E0C7585196F1CAE1E22573F701CF49D82A5BF9DA2874F98CEF9DDC4841F4DE3983C60277D928C704D09EC7EE8C48310B79F418023E10FE1BF74F0E8E72BD8EBADB1801F6446BA829DA17B325F2E8C085BE060A473D3B8C4EEB221BB238BD8641488FC596E7F1CF8DEA5EF96142E35FB3CF5C3C08E26EE37B7BD42C11CF3B20C632BDFABB53B589EE0CAFB58847CDDCDAD7733E12EAED97FBBEFDEAD63FF9D7F8309D70CBBB3BDBD966103322714B9271C7B2BDAB0886E535ED8F22794BFDD6DEA053D02DE8BD539A2FD58AF4F88F17A0BA6DFFFE3630F4BBAF1863B37A06C5D363BB5C33A363BB5EFBAD307E3748183991F8061B6F10975888DC0C654CBA2685C2B986E1F494AED8E6D4516012E318327D51257B7AD1558B38B24AF6EBFB6E2DE447DA942BEE465AD40AA369204CA865553D67624143CE8C19F5020BFBA15CF704950AC8588BAF1C785969FBF563B2BDE005A1BE85EAE03B1755BC3A520067EDDC3CFB0877BF0B36F08FEC6041FBB73B86155A017624DD6E8C388B6A4ADEFD3C994246E460FB623417A3516CF602C12DDAF1A5B7C217B74253F5CDC858D0EBBCEB6DB67CCB7C9729EF52E9A3CE7B2628EA863747551E2E8428D4B641ACBCE0BD87320C3C4FC455A970EC367F79A7C78795DCAE3EE98E29E3EA787D8C51C1BFB76FC89E500311B39327140FF4EF90998011C441B11B9709831302C8472D966106A9305723B4A27E0695AA168BED9C8E29B43BCC034B2201DD75B674AA9E591A7958D2EA8B7499B63AB40F5FA1DA07B92AA18D8FA58551330F59FF5E9DF3A2EB149EC6F29DC1390BFE55A0F9EFBCA134AC5B7E6E32A2758E6D2E9D3B9392AB549FC6D92E60908DBB45E8367685DA45DC522AD4FA539918A6909FA54D58AED6F125B35047A02C26AACDDC0381B7BDAD087430F1C2413803B06BA450C47CFF143D5BDF61A5EC6575B967C3713A916E14E31AFC89EC9BDFB0A0E4B942DE3886A651570325B1B40D55F9C246CA5D3DF76DED9279EC6E9A77E55C308F9471509313BCB048802271A9551710F2C746F1B251689BDC28D2CD343DD3A4A3B69853B5861C00A368AE6ADAC630DFD6BC7C664F577BA0CAC741DA8D345C6F01AE5B7BD01AC59F7EA9887AC6C3DEFB39DFF59102FDFCE35EA6BF438D7ACAFDA4C025965DAEE506B87A820A7FA3069ED01F5A4BD349E959DB2D9BBB115A7ED260FC69622BF777C8A160B42E7857CDFE489318D937D0FDE4CDBE7BC7A318665B38AD4D76CB6D9486015D11C0B6F234D3AF898048CA75E83691C389ED44CF42914A7593A9AE436C84B999E726997E8E724374923F579A4C6CED57B0C127B917FB78C4E57B34CEA6C44D9D8C845414534FCC077438FAA9D4D75EFD4292C22A81C45358A9CF95AC493DFB6402EC6BD4BA0C517FA78E5D87711B0FC46461C5BC2EA496EB4C41DE94E5266A316576513D13B659BACA606719B21D644DF38CFB3C4DEF8913E4692B459C4481EB5C01032304B60C2BB769B3349C614F767F2581FAB90A259C42A3CD6C7CA92348B48D9437D1C3949B30828BFED861CA56FAA703F4A5F00EB50BF7B63A4BC0AF56E935437E2F6A6491B69C8076C4DD65811B8A6D92BCBBB1FB9E99D73ED276F129459FDFC55010D99E3556767FB333349AC2A82248FBAB8B34CE5C9B6C3FBDE77541676E87D07A541C8F65B46D973C87BA4904C546278FEF89595C26B294E2236C946CFE225425C649CC4289A8BA3A5A045DCC4344055F7C4890216D34706366D1435184DBFBA072EC1510C3D6D708A2899012DE2AC3A73777B6757A8A61E4E65B3C598E356C478AACB9BCB2BF604C98121255F434C96997EC03EB8CF352603762896AB1B4FFABE051E197E98987F2DFBEF19277F7E8E21B68CF300167FCFD836FEEEADEC98829DB4EF50F093871E7EEE23E1D0819F795F09879A60DDCB705F22DF8A55ADE9F2CA75ADAB14AD56A22E6B1C7BA9499575B46A7D2A89AC670EA2536C2955AB7626B650ADDA194755AD5A05A8236175F5AACEDA762C561DB469D08A00BC100B318813A9B1C6ADCE0CBC4406EA5CCF5F09D81F017B3E744AC55AC281D3B554AB13CC86D1BEF28AFDCAF3FE782ED516AD4CF581536C0DC53A728AC973A5A836668FF55328D44BDDC13396D50C2DD975FDE53343A0A42A7FB487D29D8D21A4F6E79A81F1B17549CB100998A51B7729A7D9188AA9BF670C8C535D8A508648AB626267C702988D21576D6EE2000B46E41C5071153B9683C49F65C01BBDF561D593B8C3ADBB42C58856C18862D8BE2A4BDA149628A6D257214A7AF46B97A168AAA65DD54A5DD18A62C04D2A6A5171B03A23B343B58A4E19CC4BAB4DE9AEC4FA3D5097A633F81293FE9424ECCC5226C686548EF4A70CF9BC6AA835E9A71C444E678093BBF017E1C16F60649E43447F1F9E62BB7466676D4EE8CC4F5D0761466913312C843972E040DF0F3899219BC36B1B33B6FC727D83DC109A1C79B7D839A1E7215F841C44C6DEAD5BCAD48D5C90BAF197352FE5398FCF17CBBF45D38708304D1245B2CEE9C790B84E36EFE38A48960222F26D927864FCE51EE0E68F19D2994F358112F5652ED915F6162E80B1733A45F7B8CBDCAE19FE84E7C87E4CB352D420CD0B5156FBF890A079803C9660E4FDE157E0B0E33DBCFF1F5AF37BE918610000 , N'6.1.3-40302')
END





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

---Store Procedure GetFilteredTestResults tblMultivariateTests.
IF EXISTS (SELECT * FROM SYS.OBJECTS WHERE object_id = object_id(N'[dbo].[MultivariateTest_GetFilteredTestResults]') and OBJECTPROPERTY(object_id, N'IsProcedure') = 1)
    DROP PROCEDURE [dbo].[MultivariateTest_GetFilteredTestResults]
GO

CREATE PROCEDURE [dbo].[MultivariateTest_GetFilteredTestResults]
AS	
SET NOCOUNT ON	
BEGIN
	SELECT * FROM dbo.tblMultivariateTests
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

	if(not exists(SELECT * FROM dbo.tblMultivariateTestsResults WHERE [TestId] = @TestId and [ItemId] = @ItemId))
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
