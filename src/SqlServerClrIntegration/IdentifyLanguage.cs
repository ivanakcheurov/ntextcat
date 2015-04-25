using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Data.SqlTypes;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Ionic.Zip;
using Microsoft.SqlServer.Server;

public partial class UserDefinedFunctions
{
    private static readonly Dictionary<string, LanguageIdentifier> _languageModelsCache = new Dictionary<string, LanguageIdentifier> { { "Wikipedia-MostCommon-Utf8", LoadLanguageIdentifier("Wikipedia-MostCommon-Utf8") } };
    private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    private static readonly LanguageIdentifier.LanguageIdentifierSettings settings =
        new LanguageIdentifier.LanguageIdentifierSettings(50);

    private static IEnumerable<Tuple<string, Stream>> LoadLanguageModels(string languageModelsDirectory)
    {
        Stream langModels = Assembly.GetExecutingAssembly().GetManifestResourceStream("SqlServerClrIntegration.LanguageModels.zip");
        using (var zip = ZipFile.Read(langModels))
        {
            foreach (ZipEntry zipEntry in zip)
            {
                if (zipEntry.IsDirectory == false && zipEntry.FileName.StartsWith(languageModelsDirectory))
                {
                    MemoryStream stream = new MemoryStream();
                    zipEntry.Extract(stream);
                    stream.Position = 0;
                    yield return Tuple.Create(Path.GetFileNameWithoutExtension(zipEntry.FileName), (Stream)stream);
                }
            }
        }
    }

    private static LanguageIdentifier LoadLanguageIdentifier(string languageModelsDirectory)
    {
        IEnumerable<Tuple<string, Stream>> namesAndLanguageModelStreams = LoadLanguageModels(languageModelsDirectory);
        return new LanguageIdentifier(namesAndLanguageModelStreams);
    }

    private static LanguageIdentifier GetLanguageModels(string languageModelsDirectory)
    {
        _lock.EnterReadLock();
        
        try
        {
            LanguageIdentifier obj;
            if (_languageModelsCache.TryGetValue(languageModelsDirectory, out obj))
                return obj;
        }
        finally
        {
            _lock.ExitReadLock();
        }
        
        _lock.EnterWriteLock();
        try
        {
            LanguageIdentifier obj;
            if (_languageModelsCache.TryGetValue(languageModelsDirectory, out obj))
                return obj;
            obj = LoadLanguageIdentifier(languageModelsDirectory);
            _languageModelsCache.Add(languageModelsDirectory, obj);
            return obj;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    #region String Language Identification

    private static string IdentifyLanguageImpl(string inputText, string languageModelsDirectory, LanguageIdentifier.LanguageIdentifierSettings settings)
    {
        var languageIdentifier = GetLanguageModels(languageModelsDirectory);
        var classifyText = languageIdentifier.ClassifyText(inputText, settings);
        return classifyText.Select(t => t.Item1).FirstOrDefault();
    }

    [SqlFunction]
    public static SqlString IdentifyLanguage(SqlString inputText, SqlString languageModelsDirectory)
    {
        if (languageModelsDirectory.IsNull || string.IsNullOrEmpty(languageModelsDirectory.Value))
            throw new ArgumentException("Cannot be empty or null", "languageModelsDirectory");
        return new SqlString(IdentifyLanguageImpl(inputText.Value, languageModelsDirectory.Value, settings));
    }

    [SqlFunction]
    public static SqlString IdentifyLanguageEx(SqlString inputText, SqlString languageModelsDirectory, 
        int tooManyLanguagesThreshold, int occuranceNumberThreshold, long onlyReadFirstNLines, double worstAcceptableThreshold, int maxNgramLength)
    {
        return new SqlString(IdentifyLanguageImpl(inputText.Value, languageModelsDirectory.Value, 
            new LanguageIdentifier.LanguageIdentifierSettings(
                tooManyLanguagesThreshold, occuranceNumberThreshold, onlyReadFirstNLines, worstAcceptableThreshold, maxNgramLength)));
    }

    private static IEnumerable<Tuple<string, double>> IdentifyLanguageTableImpl(string inputText, string languageModelsDirectory,
        LanguageIdentifier.LanguageIdentifierSettings settings)
    {
        var languageIdentifier = GetLanguageModels(languageModelsDirectory);
        // Put your code here
        var classifyText = languageIdentifier.ClassifyText(inputText, settings);
        return classifyText;
    }


    [SqlFunction(
        FillRowMethodName = "FillLanguage",
        TableDefinition="Language nvarchar(10), Score float")]
    public static IEnumerable IdentifyLanguageTable(SqlString inputText, SqlString languageModelsDirectory)
    {
        if (languageModelsDirectory.IsNull || string.IsNullOrEmpty(languageModelsDirectory.Value))
            throw new ArgumentException("Cannot be empty or null", "languageModelsDirectory");
        return IdentifyLanguageTableImpl(inputText.Value, languageModelsDirectory.Value, settings);
    }

    [SqlFunction(
        FillRowMethodName = "FillLanguage",
        TableDefinition = "Language nvarchar(10), Score float")]
    public static IEnumerable IdentifyLanguageTableEx(SqlString inputText, SqlString languageModelsDirectory,
        int tooManyLanguagesThreshold, int occuranceNumberThreshold, long onlyReadFirstNLines, double worstAcceptableThreshold, int maxNgramLength)
    {
        if (languageModelsDirectory.IsNull || string.IsNullOrEmpty(languageModelsDirectory.Value))
            throw new ArgumentException("Cannot be empty or null", "languageModelsDirectory");
        return IdentifyLanguageTableImpl(inputText.Value, languageModelsDirectory.Value,
            new LanguageIdentifier.LanguageIdentifierSettings(
                tooManyLanguagesThreshold, occuranceNumberThreshold, onlyReadFirstNLines, worstAcceptableThreshold, maxNgramLength));
    }

    public static void FillLanguage(Object obj, out SqlString language, out SqlDouble score)
    {
        var languageScore = (Tuple<string, double>)obj;
        language = new SqlString(languageScore.Item1);
        score = new SqlDouble(languageScore.Item2);
    }

    #endregion

    #region Byte Stream Language Identification

    private static IEnumerable<Tuple<string, double>> IdentifyLanguageAndEncodingTableImpl(byte[] inputText, string languageModelsDirectory,
        LanguageIdentifier.LanguageIdentifierSettings settings)
    {
        var languageIdentifier = GetLanguageModels(languageModelsDirectory);
        // Put your code here
        var classifyText = languageIdentifier.ClassifyBytes(inputText, null, settings);
        return classifyText;
    }


    [SqlFunction(
        FillRowMethodName = "FillLanguageAndEncoding",
        TableDefinition = "Language nvarchar(10), Encoding int, Score float")]
    public static IEnumerable IdentifyLanguageAndEncodingTable(SqlBinary inputText, SqlString languageModelsDirectory)
    {
        if (languageModelsDirectory.IsNull || string.IsNullOrEmpty(languageModelsDirectory.Value))
            throw new ArgumentException("Cannot be empty or null", "languageModelsDirectory");
        return IdentifyLanguageAndEncodingTableImpl(inputText.Value, languageModelsDirectory.Value, settings);
    }

    [SqlFunction(
        FillRowMethodName = "FillLanguageAndEncoding",
        TableDefinition = "Language nvarchar(10), Encoding int, Score float")]
    public static IEnumerable IdentifyLanguageAndEncodingTableEx(SqlBinary inputText, SqlString languageModelsDirectory,
        int tooManyLanguagesThreshold, int occuranceNumberThreshold, long onlyReadFirstNLines, double worstAcceptableThreshold, int maxNgramLength)
    {
        if (languageModelsDirectory.IsNull || string.IsNullOrEmpty(languageModelsDirectory.Value))
            throw new ArgumentException("Cannot be empty or null", "languageModelsDirectory");
        return IdentifyLanguageAndEncodingTableImpl(inputText.Value, languageModelsDirectory.Value,
            new LanguageIdentifier.LanguageIdentifierSettings(
                tooManyLanguagesThreshold, occuranceNumberThreshold, onlyReadFirstNLines, worstAcceptableThreshold, maxNgramLength));
    }

    public static void FillLanguageAndEncoding(Object obj, out SqlString language, out SqlInt32 encoding, out SqlDouble score)
    {
        var languageEncodingScore = (Tuple<string, double>)obj;
        var languageEncoding = languageEncodingScore.Item1;
        const string codepageSeparator = "_cp";
        var lastIndexOfEncoding = languageEncoding.LastIndexOf(codepageSeparator, StringComparison.Ordinal);
        var encodingString = languageEncoding.Substring(lastIndexOfEncoding + codepageSeparator.Length);
        int encodingInt;
        if (int.TryParse(encodingString, out encodingInt))
        {
            language = new SqlString(languageEncodingScore.Item1.Remove(lastIndexOfEncoding));
            encoding = new SqlInt32(encodingInt);
        }
        else
        {
            language = new SqlString(languageEncodingScore.Item1);
            encoding = new SqlInt32(0);
        }
        score = new SqlDouble(languageEncodingScore.Item2);
    }

    #endregion
}

