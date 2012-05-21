-- Examples for queries that exercise different SQL objects implemented by this assembly

-----------------------------------------------------------------------------------------
-- Stored procedure
-----------------------------------------------------------------------------------------
-- exec StoredProcedureName


-----------------------------------------------------------------------------------------
-- User defined function
-----------------------------------------------------------------------------------------
-- select dbo.FunctionName()


-----------------------------------------------------------------------------------------
-- User defined type
-----------------------------------------------------------------------------------------
-- CREATE TABLE test_table (col1 UserType)
--
-- INSERT INTO test_table VALUES (convert(uri, 'Instantiation String 1'))
-- INSERT INTO test_table VALUES (convert(uri, 'Instantiation String 2'))
-- INSERT INTO test_table VALUES (convert(uri, 'Instantiation String 3'))
--
-- select col1::method1() from test_table



-----------------------------------------------------------------------------------------
-- User defined type
-----------------------------------------------------------------------------------------
-- select dbo.AggregateName(Column1) from Table1


--select 'To run your project, please edit the Test.sql file in your project. This file is located in the Test Scripts folder in the Solution Explorer.'

DECLARE @text NVARCHAR(MAX)
SET @text = N'Вболівальники, які придбали квитки на матчі Євро-2012 о 21:45, матимуть можливість подивитися на стадіоні відразу два матчі, якщо прийдуть на арену заздалегідь.'
select dbo.IdentifyLanguage(@text, N'Wikipedia-MostCommon-Utf8') as idl
select dbo.IdentifyLanguageEx(@text, N'Wikipedia-MostCommon-Utf8', 50, 0, 1194329582398, 1.05, 5) as idl
select * from dbo.IdentifyLanguageTable(@text, N'Wikipedia-MostCommon-Utf8')
select * from dbo.IdentifyLanguageTableEx(@text, N'Wikipedia-MostCommon-Utf8', 50, 0, 1194329582398, 1.05, 5)

select * from dbo.IdentifyLanguageAndEncodingTableEx(0x48657861646563696d616c206e6f746174696f6e206973207573656420617320612068756d616e2d667269656e646c7920726570726573656e746174696f6e206f662062696e6172792076616c75657320696e20636f6d70757465722070726f6772616d6d696e6720616e64206469676974616c20656c656374726f6e6963732e204d6f73742070726f6772616d6d696e67206c616e6775616765732073756368206173204a6176612c204153502e4e45542c20432b2b2c20466f727472616e206574632068617665206275696c742d696e2066756e6374696f6e73207468617420, N'Wikipedia-MostCommon-Legacy__All-Utf8', 250, 0, 1194329582398, 1.05, 5)
select * from dbo.IdentifyLanguageAndEncodingTable(CAST(@text AS VARBINARY), N'Wikipedia-MostCommon-Legacy__All-Utf8')
select * from dbo.IdentifyLanguageAndEncodingTableEx(CAST(@text AS VARBINARY), N'Wikipedia-MostCommon-Legacy__All-Utf8', 50, 0, 1194329582398, 1.05, 5)



