using System;
using System.Collections.Generic;
using System.Linq;
using NClassify;
using NTextCatLib;

namespace NTextCatLibLegacy
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// OPTIMIZATION. Legacy text cat works on level of bytes (not chars, so it is encoding tolerant).
    /// Legacy text cat's Ngrams are 5-grams at max. So we could employ UInt64 (8 bytes) to hold 5 significant bytes.
    /// To go beyond 8 bytes, remove alias NGram and create type NGram which will hold sequence of bytes of any length, override <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>
    /// </remarks>
    public class LegacyLanguageGuesser : ICategorizedClassifier<IDistribution<ulong>, string>
    {
        private Dictionary<IDictionary<ulong, int>, string> _etalonLanguageModel2languageName = new Dictionary<IDictionary<ulong, int>, string>();

        private int _defaultNgramRankOnAbsence;

        public LegacyLanguageGuesser(int defaultNgramRankOnAbsence)
        {
            _defaultNgramRankOnAbsence = defaultNgramRankOnAbsence;
        }

        public void AddEtalonLanguageModel(string name, IDistribution<ulong> languageModel)
        {
            _etalonLanguageModel2languageName.Add(GetRankedLanguageModel(languageModel), name);
        }

        public IEnumerable<Tuple<string, double>> Classify(IDistribution<ulong> guessedLanguageModel)
        {
            IDictionary<ulong, int> rankedGuessedLanguageModel = GetRankedLanguageModel(guessedLanguageModel);
            var classifier =
                new KnnMonoCategorizedClassifier<IDictionary<ulong, int>, string>(
                    new LegacyLanguageModelDistanceCalculator(rankedGuessedLanguageModel, _defaultNgramRankOnAbsence),
                    _etalonLanguageModel2languageName);
            IEnumerable<Tuple<string, double>> likelyLanguages = classifier.Classify(rankedGuessedLanguageModel);
            return likelyLanguages;
        }

        private static IDictionary<ulong, int> GetRankedLanguageModel(IDistribution<ulong> languageModel)
        {
            Dictionary<ulong, int> rankedLanguageModel = languageModel.OrderByDescending(e => e.Value).Select((e, i) => new { event_ = e.Key, rank = i + 1 }).ToDictionary(p => p.event_, p => p.rank);
            return rankedLanguageModel;
        }
    }
}
