using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NTextCat.NClassify
{
    public class VectorDistanceCalculator<T> : IDistanceCalculator<IDistribution<T>>
    {
        public double CalculateDistance(IDistribution<T> obj1, IDistribution<T> obj2)
        {
            double result =
                obj1.DistinctRepresentedEvents.Union(obj2.DistinctRepresentedEvents)
                    .Select(ngram => Math.Abs(obj1.GetEventFrequency(ngram) - obj2.GetEventFrequency(ngram)))
                    .Sum() / 2;
            return result;
        }
    }
}
