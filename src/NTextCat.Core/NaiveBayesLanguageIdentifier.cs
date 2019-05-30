using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NTextCat.NClassify;

namespace NTextCat.Core
{
    public class NaiveBayesLanguageIdentifier
    {
        public int MaxNGramLength { get; private set; }
        public int OnlyReadFirstNLines { get; set; }
        private NaiveBayesClassifier<IEnumerable<string>, string, LanguageInfo> _classifier;

        public NaiveBayesLanguageIdentifier(IEnumerable<LanguageModel<string>> languageModels,  int maxNGramLength, int onlyReadFirstNLines)
        {
            MaxNGramLength = maxNGramLength;
            OnlyReadFirstNLines = onlyReadFirstNLines;
            _classifier = new NaiveBayesClassifier<IEnumerable<string>, string, LanguageInfo>(
                languageModels.ToDictionary(lm => lm.Language, lm => lm.Features), 1);
        }

        public IEnumerable<Tuple<LanguageInfo, double>> Identify(string text)
        {
            var extractor = new CharacterNGramExtractor(MaxNGramLength, OnlyReadFirstNLines);
            var tokens = extractor.GetFeatures(text);
            return _classifier.Classify(tokens);
        }
    }
}
