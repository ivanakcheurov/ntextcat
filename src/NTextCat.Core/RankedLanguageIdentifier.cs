using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NTextCat.NClassify;

namespace NTextCat.Core
{
    /// <summary>
    /// Identifies the language of a given text.
    /// Please use <see cref="RankedLanguageIdentifierFactory"/> to load an instance of this class from a file
    /// </summary>
    public class RankedLanguageIdentifier
    {
        //private readonly List<LanguageModel<string>> _languageModels;
        private RankedClassifier<string> _classifier;

        public int MaxNGramLength { get; }
        public int MaximumSizeOfDistribution { get; }
        public int OccuranceNumberThreshold { get; set; }
        public int OnlyReadFirstNLines { get; set; }
        private List<LanguageModel<string>> _languageModels = new List<LanguageModel<string>>();

        public IEnumerable<LanguageModel<string>> LanguageModels
        {
            get { return _languageModels; }
        }

        public RankedLanguageIdentifier(IEnumerable<LanguageModel<string>> languageModels, int maxNGramLength, int maximumSizeOfDistribution, int occuranceNumberThreshold, int onlyReadFirstNLines)
        {
            MaxNGramLength = maxNGramLength;
            MaximumSizeOfDistribution = maximumSizeOfDistribution;
            OccuranceNumberThreshold = occuranceNumberThreshold;
            OnlyReadFirstNLines = onlyReadFirstNLines;

            //_languageModels = languageModels.ToList();

            //_classifier =
            //    new KnnMonoCategorizedClassifier<IDistribution<string>, LanguageInfo>(
            //        (IDistanceCalculator<IDistribution<string>>) new RankingDistanceCalculator<IDistribution<string>>(MaximumSizeOfDistribution),
            //        _languageModels.ToDictionary(lm => lm.Features, lm => lm.Language));

            _classifier = new RankedClassifier<string>(MaximumSizeOfDistribution);
            foreach (var languageModel in languageModels)
            {
                _classifier.AddEtalonLanguageModel(languageModel);
                _languageModels.Add(languageModel);
            }
        }


        public IEnumerable<Tuple<LanguageInfo, double>> Identify(string text)
        {
            var extractor = new CharacterNGramExtractor(MaxNGramLength, OnlyReadFirstNLines);
            var tokens = extractor.GetFeatures(text);
            var model = LanguageModelCreator.CreateLangaugeModel(tokens, OccuranceNumberThreshold, MaximumSizeOfDistribution);
            return _classifier.Classify(model);
        }
    }
}
