using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NTextCat.NClassify;

namespace NTextCat.Core
{
    /// <summary>
    /// A non-linear distance calculator which calculates distance between two distributions
    /// </summary>
    /// <typeparam name="T">type of token used in Language Models</typeparam>
    public class RankingDistanceCalculator<T> : /*IDistanceCalculator<IDistribution<T>>, */IDistanceCalculator<IDictionary<T, int>>
    {
        private int _defaultRankDistanceOnAbsence;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="defaultRankDistanceOnAbsence">if ngram is absent in known language model, this number will be used as rank distance for this ngram between unknown and known language models</param>
        public RankingDistanceCalculator(int defaultRankDistanceOnAbsence)
        {
            _defaultRankDistanceOnAbsence = defaultRankDistanceOnAbsence;
        }

        /*public double CalculateDistance(IDistribution<T> obj1, IDistribution<T> obj2)
        {
            IDistribution<T> unknown;
            IDistribution<T> known;
            if (obj1.DistinctRepresentedEventsCount < obj2.DistinctRepresentedEventsCount)
            {
                unknown = obj1;
                known = obj2;
            }
            else
            {
                unknown = obj2;
                known = obj1;
            }

            long totalDistance = 0;
            foreach (var ngramAndRank in (IEnumerable<KeyValuePair<T,long>>) unknown)
            {
                var eventCount = known.GetEventCount(ngramAndRank.Key);
                long rankDistance = eventCount == 0
                                       ? _defaultRankDistanceOnAbsence
                                       : Math.Abs(eventCount - ngramAndRank.Value);
                totalDistance += rankDistance;
            }
            return totalDistance;
        }*/

        double IDistanceCalculator<IDictionary<T, int>>.CalculateDistance(IDictionary<T, int> obj1, IDictionary<T, int> obj2)
        {
            IDictionary<T, int> unknown;
            IDictionary<T, int> known;
            var count1 = obj1.Count;
            var count2 = obj2.Count;
            if (count1 < count2)
            {
                unknown = obj1;
                known = obj2;
            }
            else
            {
                unknown = obj2;
                known = obj1;
            }

            int totalDistance = Math.Abs(count1 - count2) * _defaultRankDistanceOnAbsence;
            foreach (var ngramAndRank in unknown)
            {
                int rank;
                int rankDistance = known.TryGetValue(ngramAndRank.Key, out rank) == false
                                       ? _defaultRankDistanceOnAbsence
                                       : Math.Abs(rank - ngramAndRank.Value);
                totalDistance += rankDistance;
            }
            return totalDistance / (double) Math.Max(count1, count2);
        }
    }
}
