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

IF @CurrentMigration < '201512091937453_AutomaticMigration'
BEGIN
    CREATE TABLE [dbo].[tblMultivariateTest] (
        [Id] [uniqueidentifier] NOT NULL DEFAULT newsequentialid(),
        [Title] [nvarchar](255) NOT NULL,
        [Owner] [nvarchar](100) NOT NULL,
        [OriginalItemId] [uniqueidentifier] NOT NULL,
        [State] [nvarchar](10) NOT NULL,
        [StartDate] [datetime] NOT NULL,
        [EndDate] [datetime] NOT NULL,
        [LastModifiedDate] [datetime],
        [LastModifiedBy] [nvarchar](100) NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [ModifiedDate] [datetime] NOT NULL,
        CONSTRAINT [PK_dbo.tblMultivariateTest] PRIMARY KEY ([Id])
    )
    CREATE TABLE [dbo].[tblConversion] (
        [Id] [uniqueidentifier] NOT NULL DEFAULT newsequentialid(),
        [TestId] [uniqueidentifier] NOT NULL,
        [ConversionString] [nvarchar](max) NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [ModifiedDate] [datetime] NOT NULL,
        CONSTRAINT [PK_dbo.tblConversion] PRIMARY KEY ([Id])
    )
    CREATE TABLE [dbo].[tblKeyPerformanceIndicator] (
        [Id] [uniqueidentifier] NOT NULL DEFAULT newsequentialid(),
        [TestId] [uniqueidentifier] NOT NULL,
        [KeyPerformanceIndicatorId] [uniqueidentifier],
        [CreatedDate] [datetime] NOT NULL,
        [ModifiedDate] [datetime] NOT NULL,
        CONSTRAINT [PK_dbo.tblKeyPerformanceIndicator] PRIMARY KEY ([Id])
    )
    CREATE TABLE [dbo].[tblMultivariateTestResult] (
        [Id] [uniqueidentifier] NOT NULL DEFAULT newsequentialid(),
        [TestId] [uniqueidentifier] NOT NULL,
        [ItemId] [uniqueidentifier] NOT NULL,
        [Views] [int],
        [Conversions] [int],
        [CreatedDate] [datetime] NOT NULL,
        [ModifiedDate] [datetime] NOT NULL,
        CONSTRAINT [PK_dbo.tblMultivariateTestResult] PRIMARY KEY ([Id])
    )
    CREATE TABLE [dbo].[tblVariant] (
        [Id] [uniqueidentifier] NOT NULL DEFAULT newsequentialid(),
        [TestId] [uniqueidentifier] NOT NULL,
        [VariantId] [uniqueidentifier] NOT NULL,
        [CreatedDate] [datetime] NOT NULL,
        [ModifiedDate] [datetime] NOT NULL,
        CONSTRAINT [PK_dbo.tblVariant] PRIMARY KEY ([Id])
    )
    CREATE INDEX [IX_TestId] ON [dbo].[tblConversion]([TestId])
    CREATE INDEX [IX_TestId] ON [dbo].[tblKeyPerformanceIndicator]([TestId])
    CREATE INDEX [IX_TestId] ON [dbo].[tblMultivariateTestResult]([TestId])
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
    VALUES (N'201512091937453_AutomaticMigration', N'EPiServer.Marketing.Multivariate.Dal.Migrations.Configuration',  0x1F8B0800000000000400ED5C516FE3B8117E2FD0FF20E8A92DF6AC248B03DAC0BEC3AE931C826E36419C5DF46DC14863873889F289542E41D15FD687FEA4FE858E6C499644512265D995B3C6BE7829F22367F4713843CDE4BFFFFECFF8E797C0B79E21E2346413FB7474625BC0DCD0A36C31B16331FFE1AFF6CF3FFDF10FE34B2F78B1BE66FDDE27FD7024E313FB4988E5B9E370F70902C2470175A39087733172C3C0215EE89C9D9CFCCD393D7500216CC4B2ACF17DCC040D60F51FFCEF34642E2C454CFC9BD0039FA7EDF864B642B53E9300F892B830B12FEFE80C225CF1E88644BF82C0858E6E625FD0671251226074417CDBFAE053826B9B813FB72DC2582888C0959F7FE1301351C816B3253610FFE17509D86F4E7C0EA944E79BEEBAC29D9C25C2399B8119941B7311068680A7EF536D39D5E19D746EE7DA447D5EA2DEC56B22F54AA713BBA8BA07E0C2B6AA939E4FFD2819A0A7FAD16A060A7C54457E67E98C7F97930CB998FC7B674DB14B1CC184412CA2A4C75DFCE853F7EFF0FA10FE0A6CC262DF2F0A8962E2B3520336DD45E11222F17A0FF354F46BCFB69CF238A73A301F5618B356C62F31C5DF9F716EF2E8434EA182DE66228CE0176010A16CDE1D1102229640C0EA25489357A67AA0C2876C36242D6ACBB66EC8CB27600BF134B1CF7EFCD1B6AEE80B78594BBA842F8CE206C641228AA16689CDD3DEFE8E0B6E98F6F4E46427D346744119F1AF0504AD4A6E869AA1FE9B1477BA130170D6485C14664E7E3FA0913346BA645E2F389F0817684FE99C8202507FFCC7D7FD53621A41B2717AD145B31EDAC03E9367BA586DEAEA1243969E9DDCB6EEC15FF5E14F74B93E7A469BE7DF64437B1585C17DE89760A46EDF1E48B400ECFD10B6F79D8571E41A2C1FADDD1D44F3304273EFC235F3A84BD068D5CBA2E8DC2898EE18494AED81A6225701EE81634BBDC4F57D1B05D61C22C9AB3BCE54DCAFC958A6902F7DD82890AA8F2481B263DD92C7CEC61169744F366CEFCF31D9601E5D12439704DFE79687F346F9D941A23C56F0E7777AACC81B7237674BB63575CE966CBF77DAC60A73DEDF9E564C70DCE07BDFE08A375145FD3EF6E84E7DA6EAEE3576B63A6DE57A4F65EB9D5C0F7BDCBF7BDFBF3D04E05F29FCCE33846B26DE9FE97B0586E3DE889DD861A451B512A6114A2723910603FD9DEF29E0D11EECDD1EA49ADFD6ED7F231B75AB90B9BA155B636B9DBDF781F3D0A5AB754AC6545E66591797CCB30CA28475A0568E3D30624BFA2F7133E1E226F65F2485EB4D925F276C2691755C9EEAD4AEEECF5B76013E08B03EB8EBAF2D53C25DE2C924405D7AE516DCD210259B8AF8B84C8E46823221EF7FCA5CBA24BEBE4015084D23922C319FACFAE40296C01203A0FFEE745691D90A7925F9841525B6E96CEC14C8D9CC596D4759C52D73AF7943346574AA4F6DF38BCE43E2B9A9747B20BDE9FB1EFC0ED07501550C34F607D504CC423A7DFA1B5F7B1F12FB0D85DB03F90DDFF5E0B9AFF4AA547C6B77B13604CB63117D3AB77FF43824FEB649B307C2B6BDAF813174ED69E3188123204A1780310679241C927678A90B6EBFE0C3757CCBD38F13558625B833100A9BCF6D6BE3E42BF925D1B60C5ABA4B91E08A9E7B0B90FAEBB084AAF4A05AA69014907F8E6DD5437648B5CCB0F9002A21E686A1025120459D566B62BFC2088D8F3355FE9A4561B98095372D6D0BB3B8AB005BC3CAAA612AAB48437DFA17E1B22EBBC506DB45070575A8B74183CA8DE3811DEB5FFB8251567F27C7742BD7B44917B9816850BEA937BA63DDABEF8C6465EB794266BE5041BC8D356C505FABF7D393BEB21BADFC9CCD9F8D9D75DE6FDA30761409C2E31BB25C52B628240CA72DD66C9D2D3CFD61669E341BAC311C97D7E4CEE6ABCD67C27D4D1650799A585D0FAE68C445E637D8D6D40BA46E55AF42719C65B3A91C07F97566A75D3632F99D261876F92057E3A3A5C857287E90F87AABCBEA0672C8105692E14D7C12D55C914F433F0E98DAFD548F4E33678B0069933E469A065BC4489B0C302A39AD25B0CA337DD434BDB5089636196164C9AA159CAC591F2B4F572D22E58DFA3872BA6A11507EDA0D39496455E17E94BEA734A1963E6914214B0FF4F1D4B2B7C93D762A3B518A90246B20059A6533A365848AEE67EFE6A710A7981B9EA6C13B323969705AB2398A80B5815252725C8957D2D32359B5C9AAF4E27B67AE2A1636A7B136D29039DD900F56046EE87664B936CB95E1D2CEDDC3F43A667B27510534648ED73978E68E5D9A365504499BBA9C205C757898E17DEF3B2A8F987BDF41D9F5A3F996518E1CF21E29E4119518BE693EB2B2F258BA20A976C967CF2F4A2A1722E3F472A2BDAC5ABAAD5877B12D54D533F5929B8AD92B479B364A3A8C66BFF9539F42727B9E75B8218CCE9116EB843AFBECE4F4AC52873D9C9A688773CFAFB9DC692A8C2EBFB73D6407C68CFE16035DA5FA210723DB6ACD0634CCA42B95183314D67D22915C64BC4D05712DEAAA60B49702615947DB140B2B16BB7D2DB087BF450FB5C09D7154B5C07580EBAAAA2EB5C1BDBDEC9A74D1CEA2EB8A5D03D6A54CF12D9A895216B13C9FF41D1CC3377899D8FF5C8D3FB7AEFFF16D0DF1CEBA8DF0A438B74EAC7FF556399891EE4F0179F9F39B679AD69DC09176FDD1AEB59EADE908D231A587C6409D80FD48C0FE08D8B3C3532ACEA289036F4857B954AB0BCA8191BE36E43EB2BC3F964B65465B137DE014EBA76E47CE29F97FA5A6D6A59B75AE19EA254F7BBFB53743CB61DD418DCD10C9A6CC3CEDA3BEE76068A8FFCD71609CECBBEA65089454652AF75071733084D4FE3C38303E1A57A20C918079627B972A9883A198FAFBD9006B47E464D0EACB6B2E8CD02A0C597FB241CFF431C4779F06638FFE1615242D05248A09B7AF31312931512CA2AF9294CC346B17A468BE05B3FA95A6F215C584C32B6F5191BA3E5958AFA245511073F8252BFD29AB658F35A69ABDB1CA93EE4A6C360B4D994C832F20E94F491563554A56D9495D889CDE802777E16FCBA3D3C0E9620391FCA579066EE9CCCEFB5CB37998B90E9515655DAAD74220888707FA8748D03971053E7681F3D5C7A9AFC48FB1CB65F008DE35BB8DC532162832048F7E29CD3C71419AE65F15BF94D73CBE5DAEFE2C4D1F22E03269729375CB3EC6D4F7F2755FD5DC64292012DF26BD8F5C7F9C43B8C56B8EF439649A40A9FA7297EC0182A58F60FC96CDC8337459DB170E9F6041DCD72C4B450DD2FE22CA6A1F5F50B28848C0538CCD78FC2F72D80B5E7EFA1F5A1E19A662610000 , N'6.1.3-40302')
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
