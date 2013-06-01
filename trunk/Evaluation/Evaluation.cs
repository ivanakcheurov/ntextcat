using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Accord.Statistics.Analysis;
using IvanAkcheurov.NTextCat.Lib;
using IvanAkcheurov.NTextCat.Lib.Legacy;
using NUnit.Framework;

namespace Evaluation
{
    [TestFixture]
    public class Evaluation
    {
        private string _languageSamplesDir = @"d:\Files\Projects\NLP\NTextCat\Datasets\Tomato";
        //private string _identifierFolder = @"d:\Files\Projects\NLP\NTextCat\Datasets\Tomato\lm_char4000\";
        private string _identifierFile = @"d:\Files\Projects\NLP\NTextCat\Datasets\Tomato\profile4000\profile.xml";
        //private string _identifierFile = @"d:\WikiDump\Wiki\extract\trainData20M\profile4000\profile.xml";
        private string _outputFolder = @"d:\Files\Projects\NLP\NTextCat\Datasets\Tomato\ConfusionMatrix";
        HashSet<string> _mostCommonLanguages = new HashSet<string> { "dan", "deu", "eng", "fra", "ita", "jpn", "kor", "nld", "nor", "por", "rus", "spa", "swe", "zho" };
        
        [Test]
        public void SanityCheck()
        {
            {
                var factory = new NaiveBayesLanguageIdentifierFactory();
                var identifier = factory.Load(_identifierFile, lm => _mostCommonLanguages.Contains(lm.Language.Iso639_2T ?? lm.Language.Iso639_3));
                var result = identifier.Identify("you got me").ToArray();
            }

            {
                var factory = new RankedLanguageIdentifierFactory();
                var identifier = factory.Load(_identifierFile, lm => _mostCommonLanguages.Contains(lm.Language.Iso639_2T ?? lm.Language.Iso639_3));
                var result = identifier.Identify("you got me").ToArray();
            }
        }

        [Test]
        [Ignore("Evaluation")]
        [Timeout(3600*1000)]
        public void Evaluate()
        {
            {
                var factory = new NaiveBayesLanguageIdentifierFactory();
                var identifier = factory.Load(_identifierFile, lm => _mostCommonLanguages.Contains(lm.Language.Iso639_2T));
                GetConfusions(identifier.Identify, "Naive", _mostCommonLanguages);
            }

            {
                var factory = new RankedLanguageIdentifierFactory();
                var identifier = factory.Load(_identifierFile, lm => _mostCommonLanguages.Contains(lm.Language.Iso639_2T));
                GetConfusions(identifier.Identify, "Ranked", _mostCommonLanguages);
            }
        }

        private void GetConfusions(Func<string, IEnumerable<Tuple<LanguageInfo, double>>> identify, string method, HashSet<string> mostCommonLanguagesArray)
        {
            var mostCommonLanguages = mostCommonLanguagesArray.Select((item, i) => new { item, i }).ToDictionary(_ => _.item, _ => _.i);
            var windowLengthList =
                Enumerable.Range(1, 10).Concat(new[] { 13, 16, 20, 23, 26, 30, 35, 40, 45, 50, 60, 70, 80, 90, 100, 120, 140, 160, 180, 200 }).ToArray();

            mostCommonLanguagesArray
                .Select(
                lang =>
                    {
                        var text = File.ReadAllText(Path.Combine(_languageSamplesDir, lang + ".txt"));
                        var middle = text.Length/2;
                        var window = 10*1000*1000;
                        // take the middle of 1M characters length
                        return Tuple.Create(lang, text.Substring(Math.Max(middle - window/2, 0), Math.Min(window - 1, text.Length)));
                    })
                .AsParallel()
                .AsOrdered()
                .SelectMany(
                _ =>
                    {
                        var lang = _.Item1;
                        var sample = _.Item2;
                        var tokenizer = new Tokenizer();
                        //printfn "tokenizing"
                        var tokenNumber = 1000;
                        var tokens = tokenizer.GetTokens(sample).Skip(5).Take(tokenNumber).ToArray();
                        //printfn "tokenized"
                        return
                            windowLengthList
                                .Select(
                                    windowLength =>
                                        {
                                            var windowCount = tokenNumber - windowLength + 1;
                                            var samplePeriod = (int) Math.Ceiling(windowCount/1000.0); //1000 samples on average
                                            var actuals =
                                                tokens.Buffer(5, samplePeriod)
                                                    .Select(tokenWindow => System.String.Join(" ", tokenWindow))
                                                    .Select(windowText => identify(windowText).First().Item1.Iso639_2T)
                                                    .ToArray();
                                            return Tuple.Create(lang, windowLength, actuals);
                                        });
                    })
                    .GroupBy(_ => _.Item2)
                    .ForEach(g =>
                                 {
                                     var windowLength = g.Key;
                                     var experiment =
                                         g.SelectMany(
                                             _ =>
                                                 {
                                                     var lang = _.Item1;
                                                     var actuals = _.Item3;
                                                     return actuals
                                                         .Select(a => Tuple.Create(mostCommonLanguages[lang], mostCommonLanguages[a]));
                                                 })
                                             .ToArray();
                                     var matrix = new GeneralConfusionMatrix(
                                         mostCommonLanguagesArray.Count, experiment.Select(_ => _.Item1).ToArray(), experiment.Select(_ => _.Item2).ToArray());
                                     using (var writer = new StreamWriter(Path.Combine(_outputFolder, windowLength + "." + method + ".csv")))
                                     {
                                        PrintMatrix(writer, matrix, mostCommonLanguagesArray.ToArray());
                                     }
                                 });
        }

        private static void PrintMatrix(TextWriter writer, GeneralConfusionMatrix matrix, string[] mostCommonLanguagesArray)
        {
            const int padding = 5;
            var langs = mostCommonLanguagesArray.Select(l => l.Length > 5 ? l.Remove(5) : l).ToArray();
            writer.Write("act->".PadRight(padding));
            foreach (var lang in langs)
            {
                writer.Write(lang.PadRight(padding));
            }

            writer.WriteLine();
            for (int i = 0; i < matrix.Matrix.GetLength(0); i++)
            {
                writer.Write(langs[i].PadRight(padding));
                for (int j = 0; j < matrix.Matrix.GetLength(1); j++)
                {
                    writer.Write(matrix.Matrix[i, j].ToString().PadRight(padding));
                }
                writer.WriteLine();
            }
        }
                    
    }
}
