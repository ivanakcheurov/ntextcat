using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IvanAkcheurov.NClassify
{
    public class VectorDistanceCalculator<T> : IDistanceCalculator<IDistribution<T>>
    {
        public double CalculateDistance(IDistribution<T> obj1, IDistribution<T> obj2)
        {
            double result =
                obj1.Events.Union(obj2.Events)
                    .Select(ngram => Math.Abs(obj1.GetEventFrequency(ngram) - obj2.GetEventFrequency(ngram)))
                    .Sum() / 2;
            return result;
        }
    }
}
