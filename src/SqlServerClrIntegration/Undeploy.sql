-- Pre-Deployment script
-- Deploys all prerequisites (references assemblies) for main assembly.
:setvar DatabaseName "Test"

--EXEC sp_configure 'clr enabled', 1;
--RECONFIGURE WITH OVERRIDE;
--GO

 
USE [master]
GO
DROP LOGIN [IvanAkcheurov.NTextCat.Login]
DROP ASYMMETRIC KEY [IvanAkcheurov.NTextCat.Key]
DROP LOGIN IonicZipLogin
DROP ASYMMETRIC KEY IonicZipKey
GO 
 
USE [$(DatabaseName)]
GO
/****** Object:  UserDefinedFunction [dbo].[IdentifyLanguage]    Script Date: 05/18/2012 18:05:31 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IdentifyLanguage]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[IdentifyLanguage]
GO
/****** Object:  UserDefinedFunction [dbo].[IdentifyLanguageAndEncodingTable]    Script Date: 05/18/2012 18:05:31 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IdentifyLanguageAndEncodingTable]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[IdentifyLanguageAndEncodingTable]
GO
/****** Object:  UserDefinedFunction [dbo].[IdentifyLanguageAndEncodingTableEx]    Script Date: 05/18/2012 18:05:31 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IdentifyLanguageAndEncodingTableEx]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[IdentifyLanguageAndEncodingTableEx]
GO
/****** Object:  UserDefinedFunction [dbo].[IdentifyLanguageEx]    Script Date: 05/18/2012 18:05:32 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IdentifyLanguageEx]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[IdentifyLanguageEx]
GO
/****** Object:  UserDefinedFunction [dbo].[IdentifyLanguageTable]    Script Date: 05/18/2012 18:05:32 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IdentifyLanguageTable]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[IdentifyLanguageTable]
GO
/****** Object:  UserDefinedFunction [dbo].[IdentifyLanguageTableEx]    Script Date: 05/18/2012 18:05:32 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IdentifyLanguageTableEx]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[IdentifyLanguageTableEx]
GO
/****** Object:  SqlAssembly [SqlServerClrIntegration]    Script Date: 05/18/2012 18:05:32 ******/
IF  EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'SqlServerClrIntegration' and is_user_defined = 1)
DROP ASSEMBLY [SqlServerClrIntegration]
GO
/****** Object:  SqlAssembly [IvanAkcheurov.NTextCat.Lib.Legacy.dll]    Script Date: 05/18/2012 18:05:32 ******/
IF  EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'IvanAkcheurov.NTextCat.Lib.Legacy.dll' and is_user_defined = 1)
DROP ASSEMBLY [IvanAkcheurov.NTextCat.Lib.Legacy.dll]
GO
/****** Object:  SqlAssembly [IvanAkcheurov.NTextCat.Lib.dll]    Script Date: 05/18/2012 18:05:32 ******/
IF  EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'IvanAkcheurov.NTextCat.Lib.dll' and is_user_defined = 1)
DROP ASSEMBLY [IvanAkcheurov.NTextCat.Lib.dll]
GO
/****** Object:  SqlAssembly [Ionic.Zip.dll]    Script Date: 05/18/2012 18:05:32 ******/
IF  EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'Ionic.Zip.dll' and is_user_defined = 1)
DROP ASSEMBLY [Ionic.Zip.dll]
GO
/****** Object:  SqlAssembly [IvanAkcheurov.Commons.dll]    Script Date: 05/18/2012 18:05:32 ******/
IF  EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'IvanAkcheurov.Commons.dll' and is_user_defined = 1)
DROP ASSEMBLY [IvanAkcheurov.Commons.dll]
GO
/****** Object:  SqlAssembly [IvanAkcheurov.NClassify.dll]    Script Date: 05/18/2012 18:05:32 ******/
IF  EXISTS (SELECT * FROM sys.assemblies asms WHERE asms.name = N'IvanAkcheurov.NClassify.dll' and is_user_defined = 1)
DROP ASSEMBLY [IvanAkcheurov.NClassify.dll]
GO