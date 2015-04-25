-- Pre-Deployment script
-- Deploys all prerequisites (references assemblies) for main assembly.
:setvar AssemblyDir "c:\your\path\to\ntextcat\folder\"
:setvar DatabaseName "Test"

EXEC sp_configure 'clr enabled', 1;
RECONFIGURE WITH OVERRIDE;
GO

USE [master]
CREATE ASYMMETRIC KEY IonicZipKey FROM EXECUTABLE FILE = '$(AssemblyDir)Ionic.Zip.dll'   
CREATE LOGIN IonicZipLogin FROM ASYMMETRIC KEY IonicZipKey   
GRANT UNSAFE ASSEMBLY TO IonicZipLogin
GO 
CREATE ASYMMETRIC KEY [IvanAkcheurov.NTextCat.Key] FROM EXECUTABLE FILE = '$(AssemblyDir)IvanAkcheurov.NClassify.dll'   
CREATE LOGIN [IvanAkcheurov.NTextCat.Login] FROM ASYMMETRIC KEY [IvanAkcheurov.NTextCat.Key]   
GRANT UNSAFE ASSEMBLY TO [IvanAkcheurov.NTextCat.Login] 
GO 

CREATE DATABASE [$(DatabaseName)]
GO

USE [$(DatabaseName)]
GO

/****** Object:  SqlAssembly [IvanAkcheurov.NClassify.dll]    Script Date: 05/18/2012 18:05:32 ******/
IF NOT EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'IvanAkcheurov.NClassify.dll' and is_user_defined = 1)
CREATE ASSEMBLY [IvanAkcheurov.NClassify.dll]
AUTHORIZATION [dbo]
FROM '$(AssemblyDir)IvanAkcheurov.NClassify.dll'
WITH PERMISSION_SET = UNSAFE
GO
/****** Object:  SqlAssembly [IvanAkcheurov.Commons.dll]    Script Date: 05/18/2012 18:05:32 ******/
IF NOT EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'IvanAkcheurov.Commons.dll' and is_user_defined = 1)
CREATE ASSEMBLY [IvanAkcheurov.Commons.dll]
AUTHORIZATION [dbo]
FROM '$(AssemblyDir)IvanAkcheurov.Commons.dll'
WITH PERMISSION_SET = SAFE
GO
/****** Object:  SqlAssembly [Ionic.Zip.dll]    Script Date: 05/18/2012 18:05:32 ******/
IF NOT EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'Ionic.Zip.dll' and is_user_defined = 1)
CREATE ASSEMBLY [Ionic.Zip.dll]
AUTHORIZATION [dbo]
FROM '$(AssemblyDir)Ionic.Zip.dll'
WITH PERMISSION_SET = UNSAFE
GO
/****** Object:  SqlAssembly [IvanAkcheurov.NTextCat.Lib.dll]    Script Date: 05/18/2012 18:05:32 ******/
IF NOT EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'IvanAkcheurov.NTextCat.Lib.dll' and is_user_defined = 1)
CREATE ASSEMBLY [IvanAkcheurov.NTextCat.Lib.dll]
AUTHORIZATION [dbo]
FROM '$(AssemblyDir)IvanAkcheurov.NTextCat.Lib.dll'
WITH PERMISSION_SET = UNSAFE
GO
/****** Object:  SqlAssembly [IvanAkcheurov.NTextCat.Lib.Legacy.dll]    Script Date: 05/18/2012 18:05:32 ******/
IF NOT EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'IvanAkcheurov.NTextCat.Lib.Legacy.dll' and is_user_defined = 1)
CREATE ASSEMBLY [IvanAkcheurov.NTextCat.Lib.Legacy.dll]
AUTHORIZATION [dbo]
FROM '$(AssemblyDir)IvanAkcheurov.NTextCat.Lib.Legacy.dll'
WITH PERMISSION_SET = UNSAFE
GO
/****** Object:  SqlAssembly [SqlServerClrIntegration]    Script Date: 05/18/2012 18:05:32 ******/
IF NOT EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'SqlServerClrIntegration' and is_user_defined = 1)
CREATE ASSEMBLY [SqlServerClrIntegration]
AUTHORIZATION [dbo]
FROM '$(AssemblyDir)SqlServerClrIntegration.dll'
WITH PERMISSION_SET = UNSAFE
GO

--ALTER ASSEMBLY [SqlServerClrIntegration]
--ADD FILE FROM --filepath
--AS N'IdentifyLanguage.cs'
--GO
--ALTER ASSEMBLY [SqlServerClrIntegration]
--ADD FILE FROM --filepath
--AS N'Properties\AssemblyInfo.cs'
--GO

--IF NOT EXISTS (SELECT * FROM ::fn_listextendedproperty(N'SqlAssemblyProjectRoot' , N'ASSEMBLY',N'SqlServerClrIntegration', NULL,NULL, NULL,NULL))
--EXEC sys.sp_addextendedproperty @name=N'SqlAssemblyProjectRoot', @value=N'D:\Files\Projects\NLP\NTextCat\AllRep\trunk\SqlServerClrIntegration' , @level0type=N'ASSEMBLY',@level0name=N'SqlServerClrIntegration'
--GO

/****** Object:  UserDefinedFunction [dbo].[IdentifyLanguageTableEx]    Script Date: 05/18/2012 18:05:32 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IdentifyLanguageTableEx]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[IdentifyLanguageTableEx](@inputText [nvarchar](4000), @languageModelsDirectory [nvarchar](4000), @tooManyLanguagesThreshold [int], @occuranceNumberThreshold [int], @onlyReadFirstNLines [bigint], @worstAcceptableThreshold [float], @maxNgramLength [int])
RETURNS  TABLE (
	[Language] [nvarchar](10) NULL,
	[Score] [float] NULL
) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SqlServerClrIntegration].[UserDefinedFunctions].[IdentifyLanguageTableEx]' 
END
GO
/****** Object:  UserDefinedFunction [dbo].[IdentifyLanguageTable]    Script Date: 05/18/2012 18:05:32 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IdentifyLanguageTable]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[IdentifyLanguageTable](@inputText [nvarchar](4000), @languageModelsDirectory [nvarchar](4000))
RETURNS  TABLE (
	[Language] [nvarchar](10) NULL,
	[Score] [float] NULL
) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SqlServerClrIntegration].[UserDefinedFunctions].[IdentifyLanguageTable]' 
END
GO
/****** Object:  UserDefinedFunction [dbo].[IdentifyLanguageEx]    Script Date: 05/18/2012 18:05:32 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IdentifyLanguageEx]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[IdentifyLanguageEx](@inputText [nvarchar](4000), @languageModelsDirectory [nvarchar](4000), @tooManyLanguagesThreshold [int], @occuranceNumberThreshold [int], @onlyReadFirstNLines [bigint], @worstAcceptableThreshold [float], @maxNgramLength [int])
RETURNS [nvarchar](4000) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SqlServerClrIntegration].[UserDefinedFunctions].[IdentifyLanguageEx]' 
END
GO
/****** Object:  UserDefinedFunction [dbo].[IdentifyLanguageAndEncodingTableEx]    Script Date: 05/18/2012 18:05:31 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IdentifyLanguageAndEncodingTableEx]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[IdentifyLanguageAndEncodingTableEx](@inputText [varbinary](8000), @languageModelsDirectory [nvarchar](4000), @tooManyLanguagesThreshold [int], @occuranceNumberThreshold [int], @onlyReadFirstNLines [bigint], @worstAcceptableThreshold [float], @maxNgramLength [int])
RETURNS  TABLE (
	[Language] [nvarchar](10) NULL,
	[Encoding] [int] NULL,
	[Score] [float] NULL
) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SqlServerClrIntegration].[UserDefinedFunctions].[IdentifyLanguageAndEncodingTableEx]' 
END
GO
/****** Object:  UserDefinedFunction [dbo].[IdentifyLanguageAndEncodingTable]    Script Date: 05/18/2012 18:05:31 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IdentifyLanguageAndEncodingTable]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[IdentifyLanguageAndEncodingTable](@inputText [varbinary](8000), @languageModelsDirectory [nvarchar](4000))
RETURNS  TABLE (
	[Language] [nvarchar](10) NULL,
	[Encoding] [int] NULL,
	[Score] [float] NULL
) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SqlServerClrIntegration].[UserDefinedFunctions].[IdentifyLanguageAndEncodingTable]' 
END
GO
/****** Object:  UserDefinedFunction [dbo].[IdentifyLanguage]    Script Date: 05/18/2012 18:05:31 ******/
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IdentifyLanguage]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
BEGIN
execute dbo.sp_executesql @statement = N'CREATE FUNCTION [dbo].[IdentifyLanguage](@inputText [nvarchar](4000), @languageModelsDirectory [nvarchar](4000))
RETURNS [nvarchar](4000) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SqlServerClrIntegration].[UserDefinedFunctions].[IdentifyLanguage]' 
END
GO