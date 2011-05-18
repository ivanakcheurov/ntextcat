using System;
using System.Collections.Generic;
using NClassify;

namespace NTextCatLibLegacy
{
    public class LegacyLanguageModelDistanceCalculator : IDistanceCalculator<IDictionary<ulong, int>>
    {
        /// <summary>
        /// HACK! Read remarks for the constructor parameter guessedLanguageModel
        /// </summary>
        private IDictionary<ulong, int> _guessedLanguageModel;

        private int _defaultRankDistanceOnAbsence;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guessedLanguageModel">
        /// HACK! Put guessed language model here!
        /// <see cref="IDistanceCalculator"/> is supposed to satisfy triangular inequity, 
        /// hence distance(obj1, obj2) should always equal distance(obj2, obj1).
        /// But it is not the case with distance comparison in original TextCat.
        /// To repeat original behavior, you should know which object is guessed language!
        /// </param>
        /// <param name="defaultRankDistanceOnAbsence">if ngram is absent in known language model, this number will be used as rank distance for this ngram between unknown and known language models</param>
        public LegacyLanguageModelDistanceCalculator(IDictionary<ulong, int> guessedLanguageModel, int defaultRankDistanceOnAbsence)
        {
            _guessedLanguageModel = guessedLanguageModel;
            _defaultRankDistanceOnAbsence = defaultRankDistanceOnAbsence;
        }

        public double CalculateDistance(IDictionary<ulong, int> obj1, IDictionary<ulong, int> obj2)
        {
            // HACK! Read remarks for the constructor parameter guessedLanguageModel
            IDictionary<ulong, int> unknown;
            IDictionary<ulong, int> known;
            if (ReferenceEquals(obj1, _guessedLanguageModel))
            {
                unknown = obj1;
                known = obj2;
            }
            else if (ReferenceEquals(obj2, _guessedLanguageModel))
            {
                unknown = obj2;
                known = obj1;
            }
            else throw new InvalidOperationException("Cannot compare two objects if none of them is predefined guessed object. For more details please read remarks for the constructor parameter guessedLanguageModel");
            
            int totalDistance = 0;
            foreach (var ngramAndRank in unknown)
            {
                int rank;
                int rankDistance = known.TryGetValue(ngramAndRank.Key, out rank) == false
                                       ? _defaultRankDistanceOnAbsence
                                       : Math.Abs(rank - ngramAndRank.Value);
                totalDistance += rankDistance;
            }
            return totalDistance;
        }
    }
}
