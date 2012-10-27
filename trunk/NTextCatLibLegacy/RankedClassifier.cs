using System;
using System.Collections.Generic;
using System.Linq;
using IvanAkcheurov.NClassify;
using IvanAkcheurov.NTextCat.Lib;

namespace IvanAkcheurov.NTextCat.Lib.Legacy
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// OPTIMIZATION. Legacy text cat works on level of bytes (not chars, so it is encoding tolerant).
    /// Legacy text cat's Ngrams are 5-grams at max. So we could employ UInt64 (8 bytes) to hold 5 significant bytes.
    /// To go beyond 8 bytes, remove alias NGram and create type NGram which will hold sequence of bytes of any length, override <see cref="object.GetHashCode"/> and <see cref="object.Equals(object)"/>
    /// </remarks>
    /// <typeparam name="T">type of event of distribution</typeparam>
    public class RankedClassifier<T> : ICategorizedClassifier<IDistribution<T>, LanguageInfo>
    {
        private Dictionary<IDictionary<T, int>, LanguageInfo> _etalonLanguageModel2languageName = new Dictionary<IDictionary<T, int>, LanguageInfo>();

        private int _defaultNgramRankOnAbsence;

        public RankedClassifier(int defaultNgramRankOnAbsence)
        {
            _defaultNgramRankOnAbsence = defaultNgramRankOnAbsence;
        }

        public RankedClassifier(IEnumerable<LanguageModel<T>> languageModels, int defaultNgramRankOnAbsence)
        {
            _defaultNgramRankOnAbsence = defaultNgramRankOnAbsence;
            foreach (var languageModel in languageModels)
            {
                AddEtalonLanguageModel(languageModel);
            }
        }

        public void AddEtalonLanguageModel(LanguageModel<T> languageModel)
        {
            _etalonLanguageModel2languageName.Add(GetRankedLanguageModel(languageModel.Features), languageModel.Language);
        }

        public IEnumerable<Tuple<LanguageInfo, double>> Classify(IDistribution<T> guessedLanguageModel)
        {
            IDictionary<T, int> rankedGuessedLanguageModel = GetRankedLanguageModel(guessedLanguageModel);
            var classifier =
                new KnnMonoCategorizedClassifier<IDictionary<T, int>, LanguageInfo>(
                    new LegacyLanguageModelDistanceCalculator<T>(rankedGuessedLanguageModel, _defaultNgramRankOnAbsence),
                    _etalonLanguageModel2languageName);
            IEnumerable<Tuple<LanguageInfo, double>> likelyLanguages = classifier.Classify(rankedGuessedLanguageModel);
            return likelyLanguages;
        }

        private static IDictionary<T, int> GetRankedLanguageModel(IDistribution<T> languageModel)
        {
            Dictionary<T, int> rankedLanguageModel = languageModel.OrderByDescending(e => e.Value).Select((e, i) => new { event_ = e.Key, rank = i + 1 }).ToDictionary(p => p.event_, p => p.rank);
            return rankedLanguageModel;
        }
    }
}
