using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IvanAkcheurov.NTextCat.Lib
{
    public class NaiveBayesLanguageIdentifierFactory
    {
        public int OccuranceNumberThreshold { get; set; }
        public int MaximumSizeOfDistribution { get; set; }
        public int OnlyReadFirstNLines { get; set; }
        public int MaxNGramLength { get; set; }

        public NaiveBayesLanguageIdentifierFactory()
        {
            OnlyReadFirstNLines = int.MaxValue;
        }

        public NaiveBayesLanguageIdentifier Train(IEnumerable<Tuple<LanguageInfo, TextReader>> input)
        {
            var result = new NaiveBayesLanguageIdentifier(TrainModels(input), MaxNGramLength, OnlyReadFirstNLines);
            return result;
        }

        public IEnumerable<LanguageModel<string>> TrainModels(IEnumerable<Tuple<LanguageInfo, TextReader>> input)
        {
            return (from languageAndText in input
                    let tokens = new CharacterNGramExtractor(5, OnlyReadFirstNLines).GetFeatures(languageAndText.Item2)
                    let distribution = LanguageModelCreator.CreateLangaugeModel(tokens, OccuranceNumberThreshold, MaximumSizeOfDistribution)
                    select new LanguageModel<string>(distribution, languageAndText.Item1)).ToList();
        }

        public NaiveBayesLanguageIdentifier TrainAndSave(IEnumerable<Tuple<LanguageInfo, TextReader>> input, Stream outputStream)
        {
            var languageModels = TrainModels(input).ToList();
            XmlProfilePersister.Save(languageModels, MaximumSizeOfDistribution, MaxNGramLength, outputStream);
            return new NaiveBayesLanguageIdentifier(languageModels, MaxNGramLength, OnlyReadFirstNLines);
        }

        public NaiveBayesLanguageIdentifier Load(Stream inputStream, Func<LanguageModel<string>, bool> filterPredicate = null)
        {
            filterPredicate = filterPredicate ?? (_ => true);
            int maxNGramLength;
            int maximumSizeOfDistribution;
            var languageModelList = 
                XmlProfilePersister.Load<string>(inputStream, out maximumSizeOfDistribution, out maxNGramLength)
                    .Where(filterPredicate);

            return new NaiveBayesLanguageIdentifier(languageModelList, maxNGramLength, OnlyReadFirstNLines);
        }
    }
}
