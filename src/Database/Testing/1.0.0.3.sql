﻿--beginvalidatingquery
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
        WHERE [Extent1].[ContextKey] = N'Testing.Migrations.Configuration'
        )  AS [Project1]
        ORDER BY [Project1].[MigrationId] DESC)

IF @CurrentMigration IS NULL
    SET @CurrentMigration = '1'

IF @CurrentMigration < '201701191938428_KpiFinancialResultCultures'
BEGIN
    
ALTER TABLE [dbo].[tblABKeyFinancialResult] ADD [TotalMarketCulture] [nvarchar](max) NOT NULL DEFAULT ''
ALTER TABLE [dbo].[tblABKeyFinancialResult] ADD [ConvertedTotal] [decimal](18, 2) NOT NULL DEFAULT 0
ALTER TABLE [dbo].[tblABKeyFinancialResult] ADD [ConvertedTotalCulture] [nvarchar](max) NOT NULL DEFAULT ''
INSERT [dbo].[__MigrationHistory]([MigrationId], [ContextKey], [Model], [ProductVersion])
VALUES (N'201701191938428_KpiFinancialResultCultures', N'Testing.Migrations.Configuration',  0x1F8B0800000000000400ED5D4B6FE4B811BE07C87F10744A8259B73D73D918DDBBF0F8B168643C36A63D932097012DB1DBC44A54AF48796D04F96539E427E52FA4D47AF1214AA4247BDC18630FDBE6E3238B2C56154B559CFFFDE7BFF39F1FE2C8BBC72923095DF8470787BE87699084846E167EC6D73FFCE8FFFCD31FFF303F0FE307EF4BD5EE5DDE0E7A52B6F0EF38DF1ECF662CB8C33162073109D284256B7E1024F10C85C9ECEDE1E15F674747330C103E6079DEFC53463989F1EE0FF8F334A101DEF20C4597498823569643CD6A87EA7D4431665B14E0857F7E4D563885191F5CA2F457CC61A2073798EDFE7F8622DF3B89088269AD70B4F63D4469C21187491F7F6678C5D3846E565B2840D1CDE31643BB358A182E89396E9ADBD275F836A76BD674ACA0828CF12476043C7A572ED44CED3E68B9FD7A216129CF61C9F9634EF56E39173EACD6C9FB7CE97C4F1DEDF8344AF396BDCB7D50A0EEB6EDA0067CE3954DDED42C039C95FFF7C63BCD229EA5784171C65314BDF1AEB3DB88047FC38F37C9AF982E681645E2BC61E650271540D1759A6C71CA1F3FE17549CD32F4BD99DC6FA676ACBB097D0A327FC908FCFE0863A3DB08D75C31EBEC7E4378842B04E02D20D8F72ED1C3074C37FC6EE1C34FDFBB200F38AC4A4AD8CF94C011834E3CCDB0F3B067980529D9165C317AF0EEB1AE7EA7387D7E12AF52B22114454B8EE3919BB4028E6E36098442CEA6397B9615EE70293F1320F3DF3720CB9C91CE693809CE354C880464BB3BB8D7380D30E56853E32E297FF7D619F403621C0E3559131CBE7FEC6080A3C3C309D8ECFC618B038EC32F84119EA4A709E80785806E809320D71E1DDD1DE9078DB426216842FC01DFE3A8DEA4048495FB16FD731524291E09B2642BB2A1B023016AA87B9F241146D49DBE1403BF4DC380159F0C03FB88EEC9A6E05D1916843770F33A49412F05784943201C3697F9DE271CED3AB03BB26DCEB4A1FD5741CD5DA449FC29890ADDD7DBFEEB0D4A37385FEAC4A1D32AC9E008DA93F905A50436D44856596F2443AB6F9BB6DEA86D9AF3596323F4590E868598D094308CF0BDD81640E44808C302AAA87B2F288493F15C92A13A3D4E92A13A97438F5C7988273C6225E2EB91B2D6C1E3EDD11CA25CE57126CA92FD9D50C13A1F6C0A24B4BCFFB37113FA42F0EF2321966CC75EEC0E87DF8185D32BB8DC757F8B60321B080EF384A3790117321A10147DC20C8442971DA634FD2A0A2F55D29ADB1AECAF8E0EAEB65701F8054519EE274A68D649507B3B033186C6135867CA324D6B9829E0DF8902F9DB968C5541B0F6CD65120724CEFFBC4EE157E980FDD1F75601CA01DDA5E70EBDD8C272D59FDF7353681310BC4F4BAA3CCC37A3B63CAF23B9623F54552DF09E5AECB71BD7167A6284B414E4F0B49252007E9592D6A72ACAC67ACB5E4FE684B64BFB89EC3174EC4E6316CB6751F0CC2FD9458436CD27BA698E653E2493BE018C3E97A044429C468F00215E7EE40DBCC4F12D4EAB234651C0C93D10B55BC4857FA86DB8D4FE446E7DD4DDFA2CA14D5BDD752E23A7C11D408775FB77FACE157B24169E3096802CCE77A1D72528DC5EE4799CD3D01BE27391BEE418FD9097B073640B7B053BBFF0FFA2AD82E3E8B5F92E8D5EDDCAE4C18E7C55305FD1331C618EBD7C2373E3E714B10085FA3187D50EE51290E538C534FF620D460F032E2494EB829FD0FC135034802405CB528DE473AD47556BCEF016C391A07CC0FEDA4CA7F2E4E853AA475696B56F15E73381A57B395DBF9777F056C7255DE2A55A283B716E87FB7F5F38D548C2F370A6717FF68113BBADEE6E89676382ABB256F32AB88A591B778FCDB178119C6B45CFB3C9D7FEFDB4998A60387F43A63618ACDDBCD567BDAACC2C5DFA5C19B9CFCDB74F4CDC4DCBB33170F7FEBD3CE62DAE15D087438FC6FA461CDD2286F372FCD0E64EF80C95C5D585957E2795D572DC15E6958DBE534BCCF79A4B94AED835769531CCD1146DA046ABBA67942698A10DB53E053D28A6EF0D86996A3AC90A5EF6FA1BA02509A1C00A4CA162F77D6D16BABA7CA556397AE005AA5E8F2EAED08ECFC0FB923058CDC6AA289357D26E955B3E8DB5AE6A8FA96E6FAC0B84345CDEBD4A66DBFCA956A5C7096BE23B5B1372981129106B3AD9BDDC6661365A6CD0E04535F9CF4C0B6A63BEB81B30DA42CA32AC77117B4C96710B58B9F86A7558D7CD6745147F59309F19C2FDE79768BB25742384FF9725DEAA88FD3FFD61E51E071F1718B380B584C3D7B3AD470231863658A9CD3F3885F882A48C57EADDF74EC3586BA62A7F8326AA4693F5BBBE7F9552AADAE7BFCB88D91E97A751DE35AB7A0184C6B90D96D38C65E632F5F5F2940C14A1B4E59BC16912653135DB83E6DE6520BD085016D9634851F1229254618F5746BE8B486591038612C62E812975F6A8A57B5C042B8B9C30AA307605A72AB6C7AA03D945A4BAD01EC718C82EE21A1BD98FA3C6B68BF06A9DC32AB486B04B4BD2DAC27E84B6187711BFADDE1E5D8B7817A1B54A7BDC2AF65D84ABCAEC5194E07749D4C8550E148B1FF7246AC50A7B3CF9039F0828D7E888F3992292B5BBABA60A346F80AC59ACF48ED1FA9F5811996E9383349335D813A9AAD2E12BE92A8313D88CD2119A2D0277347B65736B36AF8DD989D9DA60D5DBB1B1B1F34B66DB363BC6DD7E9122A055A8BAC245375481D0B25AA84A9D7460130AADE8BFA6C21EAF8C881691CA2217EA8490689940A1E2551C588B03830F627A9DA7FA2587AABB5E9CA71119651C97A48F8A2207B15304814A52A72872C450A25A3540A5DEF5C837F1AAFAA96FEA86A2B64EDBD0C441B834DF3A240163FE04D231DF57A1A0F9D3A61708E2D784A1C2A013E3E50A8232A45366D4484F0CEFC67865784786D79CB26A937AF4DA39AB3861E7A543B4FF6116CD435A34C963FE937B12E6DED1D5230313EF206F70B0FA2D3A8D08CEAFEC55834B04D77838354568A5FFF6F0E8ADF29CCBCB795A65C65818B538945BDF579137EC1902B7334A7ECB70EEB1E139F3A5E39E36A1F7280DEE50FAA7183DFC798AE74A8C80CE4F928C9B5AFB3323A3174F7A7284E41C3EF69191107EF3091E19198CD3F3C8C81022DB9F18A936547F64C48D4BBA5E1451666B03677E5F6408E986D745D65182DCC1E4C7458661B4BE2D724B0650A6274C0CE6B9B684094BB0295EB2D843A12D25B8EB705A2012108B1F16FEBF76FD8FBDE53FBE16106FBCAB14F4F1B177E8FD7BFA6727BAE8B4398B7BC863EDF18BAF3C652D9FA655D02D6F300C11E3EA0B0C8304A6FEFAC290A9486F2F0CA3457B79E1FB91FF9D1EB6BD38A652D2E5F8432FE66E87CF97A63ECE9A6F4F3D9F7CFA9DA9E7E328D0925607C9DB1A6594C8DDCF936C748F7D87A7584AA31E762F7865486B869C3E25B68A6B7EF6443E9B50EFC9F27087A6AD18A31B9F317B752F52049D92555F06CFD5F1D5033364F782A7CCE1232F8F8706A799BAEDE4934A312D4D687C72EBC88CE76F961AFA0273E9A64B047D412C27A58F8D4B3FDD1B56EBFEC8FDADD9AC3D65534FEE50F755CB20AC5462574A66F101142CCBDB04B6BF7470DEDA666C7619554E399BC6690CCDF2144F53679EA771E45199A0F689A05D948FCE1AB54B1AED9AC21E6597CAECAE64090D4B166DC967B3E2C697942E3A66599423A40606EF633EA81B4936365A6F30E43E65788E5E9E16E963080D7B92BC4D3D140834B1F02F398145C0C8A681C8FF5D278A034907D76D96749D54D68032A3AA89EAB4C11C85A0A04F524ED628E0501D60C6764F7D968F979DC7B7385CD2AB8C6F330E24E3F83692B2B87293A26BFC5D72AA3CE7F9D52E76854D41024C93E47EA62BFA3E2351F3E8DA458B9FC90091DB2ABF60282FF6124C1F8E378F35D2C7DDDB6F3640E5F2D526D60D8EB71180B12BBA42C263730E73FBCCF007BC41C16315D16506E9DF0879D9E767046D5214B312A3E90F7F020F87F1C34FFF07735DBD28D06C0000 , N'6.1.3-40302')

END


GO