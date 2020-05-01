using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NTextCat.NClassify;
using Xunit;

namespace NTextCat.Test
{

    public class CustomDomainCategorizerTest
    {
        [Fact]
        public void Test()
        {
            var trainingDocuments =
                new Dictionary<string, string>
                    {
                        { "sports", File.ReadAllText(Path.Combine("..","..","..","TestData","Sports.txt")) }, 
                        { "economy", File.ReadAllText(Path.Combine("..","..","..","TestData","Economy.txt")) },
                    };
            var featureExtractor = new BagOfWordsFeatureExtractor();
            var trainedModels = new Dictionary<IDistribution<string>, string>();
            foreach (var trainingItem in trainingDocuments)
            {
                var distribution = CreateModel(featureExtractor, trainingItem.Value);
                trainedModels.Add(distribution, trainingItem.Key);
            }
            var classifier  =
                new KnnMonoCategorizedClassifier<IDistribution<string>, string>(new VectorDistanceCalculator<string>(), trainedModels);
            var resultSports = classifier.Classify(CreateModel(featureExtractor,
                "Fitch Ratings on Wednesday said Britain's latest budget proposals show commitment to its existing deficit reduction strategy and do not impact its AAA credit rating.")).ToArray();
            Assert.True(resultSports.Length >= 1);
            Assert.Equal("economy", resultSports[0].Item1);
            var resultFinance = classifier.Classify(CreateModel(featureExtractor,
                "Ryan Flannigan strikes a four off the last ball to help Scotland claim a four-wicket win over Canada in the fifth-place play-off at the qualifying tournament for the ICC World Twenty20 in Dubai.")).ToArray();
            Assert.True(resultFinance.Length >= 1);
            Assert.Equal("sports", resultFinance[0].Item1);
        }

        private static IDistribution<string> CreateModel(IFeatureExtractor<string, Tuple<string, int>> featureExtractor, string document)
        {
            var features = featureExtractor.GetFeatures(document);
            var bag = new Bag<string>();
            foreach (var feature in features)
            {
                bag.Add(feature.Item1, feature.Item2);
            }
            var distribution = new Distribution<string>(bag);
            return distribution;
        }
    }
}
