using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IvanAkcheurov.NClassify;

namespace IvanAkcheurov.NTextCat.Lib
{
    public class RankedLanguageIdentifier
    {
        private readonly List<LanguageModel<string>> _languageModels;
        private KnnMonoCategorizedClassifier<IDistribution<string>, LanguageInfo> _classifier;

        public int MaxNGramLength { get; private set; }
        public int MaximumSizeOfDistribution { get; private set; }
        public int OccuranceNumberThreshold { get; set; }
        public int OnlyReadFirstNLines { get; set; }

        public RankedLanguageIdentifier(IEnumerable<LanguageModel<string>> languageModels, int maxNGramLength, int maximumSizeOfDistribution, int occuranceNumberThreshold, int onlyReadFirstNLines)
        {
            MaxNGramLength = maxNGramLength;
            MaximumSizeOfDistribution = maximumSizeOfDistribution;
            OccuranceNumberThreshold = occuranceNumberThreshold;
            OnlyReadFirstNLines = onlyReadFirstNLines;

            _languageModels = languageModels.ToList();
            
            _classifier =
                new KnnMonoCategorizedClassifier<IDistribution<string>, LanguageInfo>(
                    new DistributionDistanceCalculator(),
                    _languageModels.ToDictionary(lm => lm.Features, lm => lm.Language));
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
