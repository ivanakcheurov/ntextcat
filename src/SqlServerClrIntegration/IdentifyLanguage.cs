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
using NTextCat;
using SqlServerClrIntegration;

public partial class UserDefinedFunctions
{
    private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    private static readonly IdentifierSettings DefaultSettings =
        new IdentifierSettings(
            embeddedProfilePath: "Core14.profile.xml", 
            maxNGramLength: 5, 
            maximumSizeOfDistribution: 4000, 
            occuranceNumberThreshold: 0, 
            onlyReadFirstNLines: int.MaxValue);

    private static readonly Dictionary<IdentifierSettings, RankedLanguageIdentifier> _languageModelsCache =
        new Dictionary<IdentifierSettings, RankedLanguageIdentifier> { { DefaultSettings, LoadLanguageIdentifier(DefaultSettings) } };
    
    private static Stream OpenInternalFile(string embeddedProfilePath)
    {
        Stream langModels = Assembly.GetExecutingAssembly().GetManifestResourceStream("SqlServerClrIntegration.LanguageModels.zip");
        using (var zip = ZipFile.Read(langModels))
        {
            foreach (ZipEntry zipEntry in zip)
            {
                if (zipEntry.IsDirectory == false && zipEntry.FileName.Equals(embeddedProfilePath))
                {
                    MemoryStream stream = new MemoryStream();
                    zipEntry.Extract(stream);
                    stream.Position = 0;
                    return stream;
                }
            }
        }
        throw new FileNotFoundException("Could file the file: " + embeddedProfilePath);
    }

    private static RankedLanguageIdentifier LoadLanguageIdentifier(IdentifierSettings settings)
    {
        var factory = new RankedLanguageIdentifierFactory(
            settings.MaxNGramLength, 
            settings.MaximumSizeOfDistribution, 
            settings.OccuranceNumberThreshold, settings.OnlyReadFirstNLines, 
            false);
        using (var stream = OpenInternalFile(settings.EmbeddedProfilePath))
        {
            var identifier = factory.Load(stream);
            return identifier;
        }
    }

    private static RankedLanguageIdentifier GetIdentifier(IdentifierSettings settings)
    {
        _lock.EnterReadLock();
        
        try
        {
            RankedLanguageIdentifier obj;
            if (_languageModelsCache.TryGetValue(settings, out obj))
                return obj;
        }
        finally
        {
            _lock.ExitReadLock();
        }
        
        _lock.EnterWriteLock();
        try
        {
            RankedLanguageIdentifier obj;
            if (_languageModelsCache.TryGetValue(settings, out obj))
                return obj;
            obj = LoadLanguageIdentifier(settings);
            _languageModelsCache.Add(settings, obj);
            return obj;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    #region String Language Identification

    private static string IdentifyLanguageImpl(string inputText, IdentifierSettings settings)
    {
        var languageIdentifier = GetIdentifier(settings);
        var classifyText = languageIdentifier.Identify(inputText);
        return classifyText.Select(t => t.Item1.Iso639_2T).FirstOrDefault();
    }

    [SqlFunction]
    public static SqlString IdentifyLanguage(SqlString inputText)
    {
        return new SqlString(IdentifyLanguageImpl(inputText.Value, DefaultSettings));
    }

    [SqlFunction]
    public static SqlString IdentifyLanguageEx(SqlString inputText, SqlString embeddedProfilePath, int maximumSizeOfDistribution, int occuranceNumberThreshold, int onlyReadFirstNLines, int maxNgramLength)
    {
        return new SqlString(IdentifyLanguageImpl(inputText.Value, 
            new IdentifierSettings(embeddedProfilePath.Value, maxNgramLength, maximumSizeOfDistribution, occuranceNumberThreshold, onlyReadFirstNLines)));
    }

    private static IEnumerable<Tuple<string, double>> IdentifyLanguageTableImpl(string inputText, IdentifierSettings settings)
    {
        var languageIdentifier = GetIdentifier(settings);
        // Put your code here
        var classifyText = languageIdentifier.Identify(inputText).Select(t => Tuple.Create(t.Item1.Iso639_2T, t.Item2));
        return classifyText;
    }


    [SqlFunction(
        FillRowMethodName = "FillLanguage",
        TableDefinition="Language nvarchar(10), Score float")]
    public static IEnumerable IdentifyLanguageTable(SqlString inputText)
    {
        return IdentifyLanguageTableImpl(inputText.Value, DefaultSettings);
    }

    [SqlFunction(
        FillRowMethodName = "FillLanguage",
        TableDefinition = "Language nvarchar(10), Score float")]
    public static IEnumerable IdentifyLanguageTableEx(SqlString inputText, SqlString embeddedProfilePath,
        int maximumSizeOfDistribution, int occuranceNumberThreshold, int onlyReadFirstNLines, int maxNgramLength)
    {
        return IdentifyLanguageTableImpl(inputText.Value,
            new IdentifierSettings(embeddedProfilePath.Value, maxNgramLength, maximumSizeOfDistribution, occuranceNumberThreshold, onlyReadFirstNLines));
    }

    public static void FillLanguage(Object obj, out SqlString language, out SqlDouble score)
    {
        var languageScore = (Tuple<string, double>)obj;
        language = new SqlString(languageScore.Item1);
        score = new SqlDouble(languageScore.Item2);
    }

    #endregion
}

