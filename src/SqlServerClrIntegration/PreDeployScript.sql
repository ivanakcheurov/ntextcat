-- Pre-Deployment script
-- Deploys all prerequisites (references assemblies) for main assembly.
:setvar AssemblyDir2 "c:\your\path\to\ntextcat\folder\"
:setvar AssemblyDir "c:\temp\"
:setvar DatabaseName "Test"

EXEC sp_configure 'clr enabled', 1;
RECONFIGURE WITH OVERRIDE;
GO

  
USE [$(DatabaseName)]
GO
DROP ASSEMBLY [IvanAkcheurov.NTextCat.Lib.Legacy.dll]
DROP ASSEMBLY [IvanAkcheurov.NTextCat.Lib.dll]
DROP ASSEMBLY [IvanAkcheurov.NClassify.dll]
DROP ASSEMBLY [IvanAkcheurov.Commons.dll]
DROP ASSEMBLY IonicZip

--USE [master]
GO
DROP LOGIN [IvanAkcheurov.NTextCat.Login]
DROP ASYMMETRIC KEY [IvanAkcheurov.NTextCat.Key]
DROP ASYMMETRIC KEY IonicZipKey
GO 
 
CREATE ASYMMETRIC KEY IonicZipKey FROM EXECUTABLE FILE = '$(AssemblyDir)Ionic.Zip.dll'   
CREATE LOGIN IonicZipLogin FROM ASYMMETRIC KEY IonicZipKey   
GRANT UNSAFE ASSEMBLY TO IonicZipLogin
GO 
CREATE ASYMMETRIC KEY [IvanAkcheurov.NTextCat.Key] FROM EXECUTABLE FILE = '$(AssemblyDir)IvanAkcheurov.NClassify.dll'   
CREATE LOGIN [IvanAkcheurov.NTextCat.Login] FROM ASYMMETRIC KEY [IvanAkcheurov.NTextCat.Key]   
GRANT UNSAFE ASSEMBLY TO [IvanAkcheurov.NTextCat.Login] 
GO 

USE [$(DatabaseName)]
GO

CREATE ASSEMBLY IonicZip
FROM '$(AssemblyDir)Ionic.Zip.dll' WITH PERMISSION_SET = UNSAFE

CREATE ASSEMBLY [IvanAkcheurov.Commons.dll]
FROM '$(AssemblyDir)IvanAkcheurov.Commons.dll'
CREATE ASSEMBLY [IvanAkcheurov.NClassify.dll]
FROM '$(AssemblyDir)IvanAkcheurov.NClassify.dll' WITH PERMISSION_SET = UNSAFE
CREATE ASSEMBLY [IvanAkcheurov.NTextCat.Lib.dll]
FROM '$(AssemblyDir)IvanAkcheurov.NTextCat.Lib.dll' WITH PERMISSION_SET = UNSAFE
CREATE ASSEMBLY [IvanAkcheurov.NTextCat.Lib.Legacy.dll]
FROM '$(AssemblyDir)IvanAkcheurov.NTextCat.Lib.Legacy.dll' WITH PERMISSION_SET = UNSAFE
GO 

--use AdventureWorksLT2008
--go

--IF EXISTS (SELECT name FROM sysobjects WHERE name = 'FindInvalidEmails')
--   DROP FUNCTION FindInvalidEmails
--go

--IF EXISTS (SELECT name FROM sys.assemblies WHERE name = 'MyClrCode')
--   DROP ASSEMBLY MyClrCode
--go

--CREATE ASSEMBLY MyClrCode FROM 'C:\FindInvalidEmails.dll'
--WITH PERMISSION_SET = SAFE -- EXTERNAL_ACCESS
--GO

--CREATE FUNCTION FindInvalidEmails(@ModifiedSince datetime) 
--RETURNS TABLE (
--   CustomerId int,
--   EmailAddress nvarchar(4000)
--)
--AS EXTERNAL NAME MyClrCode.UserDefinedFunctions.[FindInvalidEmails]
--go

--SELECT * FROM FindInvalidEmails('2000-01-01')
--go