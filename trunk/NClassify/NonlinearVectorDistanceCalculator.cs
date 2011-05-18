﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NClassify
{
    public class NonlinearVectorDistanceCalculator<T> : IDistanceCalculator<IDistribution<T>>
    {
        public double CalculateDistance(IDistribution<T> obj1, IDistribution<T> obj2)
        {
            Func<Dictionary<T, int>, T, int, int> getRankOrDefault =
                (rankedDistribution, event_, defaultRank) =>
                    {
                        int rank;
                        if (rankedDistribution.TryGetValue(event_, out rank) == false)
                            rank = defaultRank;
                        return rank;
                    };
            var rankedDistribution1 = obj1.OrderByDescending(e => e.Value).Select((e, i) => new { event_ = e.Key, rank = i + 1 }).ToDictionary(p => p.event_, p => p.rank);
            var rankedDistribution2 = obj2.OrderByDescending(e => e.Value).Select((e, i) => new { event_ = e.Key, rank = i + 1 }).ToDictionary(p => p.event_, p => p.rank);
            int distance =
                obj1.Events
                    .Union(obj2.Events)
                    .Select(
                        e =>
                        Math.Abs(getRankOrDefault(rankedDistribution1, e, 400) -
                                 getRankOrDefault(rankedDistribution2, e, 400)))
                    .Sum();
            return distance;
        }
    }
}
