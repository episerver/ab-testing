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

IF @CurrentMigration < '201512101822429_AutomaticMigration'
BEGIN
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
    VALUES (N'201512101822429_AutomaticMigration', N'EPiServer.Marketing.Multivariate.Dal.Migrations.Configuration',  0x1F8B0800000000000400ED5C5F6FDB36107F1FB0EF20E8691B5A2B4951600BEC0DAD9314C19A2688D3626F0523D10E318972452A4D30EC93ED611F695F61275B7F49512265399333A32F2E451E79C7DF1DEFC8BBFCF3D7DFE35F1E02DFBAC71123219DD887A303DBC2D40D3D4217133BE6F3973FDABFFCFCED37E3532F78B03E65FD5E25FD60246513FB8EF3E5B1E330F70E07888D02E246210BE77CE4868183BCD0393A38F8C9393C743090B08196658DAF63CA498057FF81FF4E43EAE2258F917F117AD867693B7C99ADA85A1F5080D912B978629F5E91198E60C5A30B14FD8E392C747411FB9CDCA388208E4727C8B7AD373E41B0B619F6E7B685280D39E2B0F2E38F0CCF7814D2C56C090DC8BF795C62E837473EC32947C745775DE60E8E12E69C626046CA8D190F03438287AF526939E2F04E32B77369823C4F41EEFC31E17A25D3895D16DD0D66DCB6C4498FA77E940CD013FD683503C16C24527E61E98C7F91830CB098FC7B614DA14B1CE109C5318F921E57F1AD4FDC5FF1E34DF83BA6131AFB7E99496013BE551AA0E92A0A9738E28FD7789EB27EEED996531DE78803F361A5316B61BC8B09FCFE0073A35B1FE7102AC96DC6C308BFC31447C09B778538C7114D48E0D52648930B53DD10EEE36C36002D48CBB62ED0C37B4C17FC6E621FBD7E6D5B67E4017B594BBA848F948002C3201EC5B86689CDD35E7E8505374C7B7870B0956923B22014F9E71C07AD426E111C806D067B900BEF9CF257476DA36044C44F4AA392DF3760A48CE73FA55E2F74DE23C6C11E9239C10A82FAE3DF3EF6B0A5CDD34D239CE0BC17D69BD96E23F601DD93C54A07C52586343DEA986D5D637FD587DD91E5FAA41815DF3FCB76F12C0A83EBD0AF9091BA7DBE41D10243EF9BB0BDEF2C8C23D760F9609CAE70340F23B0CE2E3EA71E7111D8987A5E149D1B19D31D2371A93DD0946591C03566D052CF717DDF4686358748FCEA8E3365F75332962AF84B3F3632A4EA2371A0EC58B7E4B153F80D8DDE4481F6FEFC8882E6DE8330F420603F373C4B0BE167E786F214819FDB700C76E0589115723B674BA69A3A674BA6EF9DD45861CEFBD369C5047B057F720557EC8448F5FFA1A35BF59944ED3576B63AA972BDA7B2B126D793DDEBEF93EB6F0FF1F22782BF32A358B91247998C7B2676628B918668254C23944E46220D06FA3BDF53827B7BF0E4F62095FCA66EFF3351D48D426651155B636B1DDD7BC358E892D53A25632A2FB32A8B53EA590651C23A50ABC61E10B125FD97A04CB0B889FD832470BD49F2EB84621259C6D5A90E6D513F2FE909F631C7D61B77FD383245CC459E0C0290A5576D0195C651A254C88765323012847259FF0975C912F9FA0C0924348D48B2C47C32F1CB095E629A1800FDBDD35945662BE495E4130A426C93D9D82981B319B3DA8EB20A5BE65E7301346574AA0F6DF38BCE5DC2B929774F007AD3FD1EBC06E8BA802A041AFB836A0066219D3EFC8DAFBD7709FD86CC3D01F80DF77AF0D8577A552ABCB5BB5805C0F258441FCEED8F1EBB84DF366E9E00B06DFB353084AE3D6D18C361048ED205408C816E11C3493B7EA80B6E3FC2C7757CCBD2C709116109DD19E60A9BCF6CAB70F295F892605B255AB94B91C8953DF71642EAD76189AAD2836A99421240FE1CDB2A87EC906A99A178009528E68641205102459D546B62BFD2088DC71911BF665158CEA0B0D3925A98C55D25B235A8140D5355441AE2D3BF089765D92D36D82C3A288943AD060D22378E07B62C7FED0B4659FC9D1CD38D5CD32659E406A241F8A6DEE89665AFBE339285ADE70999F94225F60A6BD820BE56EFA7277965375AF9399B7F1B3BEB34DDB461EC28F279C71768B9247451CAEF4D5BACD93AB977FA72669EE31AAC69382EAB4975CD579BCF047A8D1658F89A585D0F9F9188F1CC6FB0ADA91748DD44AF42719C65B3A91C07793BB3D32E1B99FC4EF309BB3CC8D5F86829E533603F487CBDD56575033864125692908D7C14D55C914F433F0EA8DAFD548F4E135DCB04D2267D1A69D66A9946DA6440434841AD1013BE19705764A356382C9AF569957254CBB44ACDFAB4F22CD532A5BC519F8E9CA55A26287FED4639C95F55D17D2BBDAB3451AD3C6D9449563EE8D353F3DEC6F7D81134528A9424AB20059C5573A3658CCA6E68EF66A814AF981BA0A6C15B323D69902A6AA619153949AE822BE9EB1EACDA60557AF3BD235715139BC3589BD29031DD90175626DCD06D8F726D942BC3A6ADBB89E9B5CCE6CEA28AD090315EE7E8993B7869FA549948DAD4E50461AAC3C38CDEFF5DA3F2C8B9770DCAAE21CD55463972C83A52CA27AA20BC68DEA352F82C5D94885DF2D9F30B13E162649C5E52B457434BB716EB2EB605A2BA275E7263317B6460D3464987D1EC8B3FF5094E6ED1B30E17889239C0629D58671F1D1C1E09E5D3C329657618F3FC9A4B9EA67AE6EABE3D4196604CC997189355CA1F6030B2ADD6AC40D302D77265300566DD3B14C9B5C19B14FED6525DD589F652D72BCB68D31A5F9260BA20A253B02A55FC7AF09BF750F1DB998EAAE2B78EA00E87F515C03A7BDBB1E0B733E7BA5CD710EB529CF81C8D422577589E4F7AFD86600D3F4CEC3F56E38FADF3DF3EAF49BCB02E2338178EAD03EBCFDEEA0533CC7D17A087EFFB48471E34D2B46E00F6B0EB0F76AD556C4D07CE73B4753AE1F91E80FD01B067F7A65292D5C1B5A929D0EA4265C7405F1B60EF51DE1FCAA5E2A28D813E7088F553AD236792FC5709A97549669D2B857AC9CE7EDA8A9BA165AE6EA1B266886053E69BF651D5B33330D47F611C1826FBAE7519022455F9C93DD4D9EC0C20B51F03078647E3FA932102304F67EF52FBB2331053BF960DB062444E011537AFB91C42AB1C64FD40039EE96D087B9F0663B7FE0675232D65238A0937AF2C31292C512CA2AF4294CC346B97A168EE8259D54A53D18A62C2E115B5A8405D9F22AC57C7A22883D9FD4295FE84D5A2638D8965CFACDEA4BB109BCD4253DED2E0CB46FA139260AC2AA9295BA906919319E0E42EFD0178701A18591424923F074FB15B39B3F33EE7741E66AE83B0A2AC8B782D8439F2E0407F137132472E87CF2E666CF538F509F93174390D6EB1774E2F63BE8C39B08C835BBF92549EB8204DF3AF4A5EAA6B1E5F2E577F8CA60F16609924B9C9BAA46F63E27BF9BACF6A6EB2142412DF26BD8F5C3FCE01B9C5634EE943483509A5E2CB5DB21B1C2C7D20C62EE90CDDE32E6BFBC8F07BBC40EE639693A226D2BE1155B18F4F085A442860298D623CFC1730EC050F3FFF0B4F60EEED07610000 , N'6.1.3-40302')
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
