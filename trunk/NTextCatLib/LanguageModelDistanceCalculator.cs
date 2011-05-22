using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IvanAkcheurov.NClassify;

namespace IvanAkcheurov.NTextCat.Lib
{
    public class LanguageModelDistanceCalculator : IDistanceCalculator<IDistribution<string>>
    {
        public double CalculateDistance(IDistribution<string> languageModel1, IDistribution<string> languageModel2)
        {

            double result =
                languageModel1.Events.Union(languageModel2.Events)
                    .Select(ngram => Math.Abs(languageModel1.GetEventFrequency(ngram) - languageModel2.GetEventFrequency(ngram)))
                    .Sum()/2;
            return result;
        }
    }
}
