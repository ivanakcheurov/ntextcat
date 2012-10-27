using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IvanAkcheurov.NClassify;

namespace IvanAkcheurov.NTextCat.Lib
{
    public class DistributionDistanceCalculator : IDistanceCalculator<IDistribution<string>>
    {
        public double CalculateDistance(IDistribution<string> languageModel1, IDistribution<string> languageModel2)
        {

            double result =
                1.0 - 
                languageModel1.Events.Intersect(languageModel2.Events)
                    .Select(ngram => Math.Min(languageModel1.GetEventFrequency(ngram), languageModel2.GetEventFrequency(ngram)))
                    .Sum();
            return result;
        }
    }
}
