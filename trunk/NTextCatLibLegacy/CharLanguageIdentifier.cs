using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IvanAkcheurov.NClassify;

namespace IvanAkcheurov.NTextCat.Lib.Legacy
{
    public class CharLanguageIdentifier
    {
        private const int MaximumSizeOfDistributionDefault = 400;
        private const string LanguageModelsDirectoryDefault = @"LM";
        private RankedClassifier<string> _classifier;

        public int MaxNGramLength { get; private set; }
        public int MaximumSizeOfDistribution { get; private set; }
        public int OccuranceNumberThreshold { get; set; }
        public int OnlyReadFirstNLines { get; set; }

        public CharLanguageIdentifier(
            string languageModelsDirectory = LanguageModelsDirectoryDefault, 
            int maximumSizeOfDistribution = MaximumSizeOfDistributionDefault
            )
        {
            MaxNGramLength = 5;
            OnlyReadFirstNLines = int.MaxValue;
            MaximumSizeOfDistribution = maximumSizeOfDistribution;
            _classifier = new RankedClassifier<string>(MaximumSizeOfDistribution);
            var persister = new LanguageModelPersister<string>();
            foreach (string filename in Directory.GetFiles(languageModelsDirectory))
            {
                using (FileStream sourceStream = File.OpenRead(filename))
                {
                    var languageModel = persister.Load(sourceStream, new LanguageInfo(Path.GetFileNameWithoutExtension(filename), null, null, null));
                    _classifier.AddEtalonLanguageModel(languageModel);
                }
            }
        }


        public IEnumerable<Tuple<LanguageInfo, double>> Identify(string text)
        {
            var extractor = new CharacterNGramExtractor(MaxNGramLength, OnlyReadFirstNLines);
            var tokens = extractor.GetFeatures(text);
            var model = LanguageModelCreator.CreateLangaugeModel(tokens, OccuranceNumberThreshold, MaximumSizeOfDistribution);
            var likelyLanguages = _classifier.Classify(model);
            return likelyLanguages;
        }
    }
}
