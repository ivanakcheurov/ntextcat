using System;
using System.Collections.Generic;
using System.Linq;
using NTextCat.NClassify;

namespace NTextCat.Core
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Used because KnnMonoCategorizedClassifier stores ranked events dictionary and not a regular distribution.
    /// Any new distribution gets converted to the ranked events dictionary and gets compared to the known language ranked events dictionaries.
    /// We could just compare distributions directly but then they have to get ranked each time 
    /// inside of RankingDistanceCalculator (including the known language distributions) which imposes a performance hit.
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
                    new RankingDistanceCalculator<T>(_defaultNgramRankOnAbsence),
                    _etalonLanguageModel2languageName);
            IEnumerable<Tuple<LanguageInfo, double>> likelyLanguages = classifier.Classify(rankedGuessedLanguageModel);
            return likelyLanguages;
        }

        private static IDictionary<T, int> GetRankedLanguageModel(IDistribution<T> languageModel)
        {
            Dictionary<T, int> rankedLanguageModel = languageModel.OrderByDescending<KeyValuePair<T, long>, long>(e => e.Value).Select((e, i) => new { event_ = e.Key, rank = i + 1 }).ToDictionary(p => p.event_, p => p.rank);
            return rankedLanguageModel;
        }
    }
}
