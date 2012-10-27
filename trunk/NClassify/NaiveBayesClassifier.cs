using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IvanAkcheurov.NClassify
{

    public class NaiveBayesClassifier<TItem, TFeature, TCategory> : ICategorizedClassifier<TItem, TCategory>
        where TItem: IEnumerable<TFeature>
    {
        private Dictionary<TCategory, DistributionAndMinEventCount<TFeature>> _categoryAndFeatures;
        private double? _kSmoothFactor;
        private long _totalDistinctRepresentedEventCount;

        public NaiveBayesClassifier(IDictionary<TCategory, IDistribution<TFeature>> knownInstances, double? kSmoothFactor)
        {
            _kSmoothFactor = kSmoothFactor;
            if (knownInstances == null) throw new ArgumentNullException("knownInstances");
            _totalDistinctRepresentedEventCount = knownInstances.Sum(d => d.Value.TotalEventCountWithNoise);
            _categoryAndFeatures =
                knownInstances.ToDictionary(
                _ => _.Key,
                distr =>
                new DistributionAndMinEventCount<TFeature>
                    {
                        Distribution = distr.Value,
                        // APPROXIMATION! Noise feature occurs on average twice less than the least frequent represented feature.
                        // It might increase significance of noise in terms of frequencies (because noise distributes according to Zipf's law rather than linear distribution.
                        // Therefore denominator of > 2.0 might be better
                        AverageNoiseFeatureFrequencyLog = Math.Log(distr.Value.Events.Select(distr.Value.GetEventCount).DefaultIfEmpty(0).Min() / 2.0 / distr.Value.TotalNoiseCount),
                        
                        TotalEventCountWithNoiseLog = Math.Log(distr.Value.TotalEventCountWithNoise),
                        TotalRepresentedEventCountWithKSmoothingLog = 
                            _kSmoothFactor.HasValue 
                            ? Math.Log(distr.Value.TotalRepresentedEventCount + _totalDistinctRepresentedEventCount * _kSmoothFactor.Value) 
                            : (double?)null,
                        CategoryFrequencyLog = Math.Log(distr.Value.TotalEventCountWithNoise) - Math.Log(_totalDistinctRepresentedEventCount),
                    });
        }

        public IEnumerable<Tuple<TCategory, double>> Classify(TItem item)
        {
            var features = item.ToList();
            var result = new List<Tuple<TCategory, double>>();
            foreach (var categoryAndFeature in _categoryAndFeatures)
            {
                var category = categoryAndFeature.Key;
                var categoryFeatures = categoryAndFeature.Value;
                double featureFreqSum = categoryFeatures.CategoryFrequencyLog;
                foreach (var feature in features)
                {
                    var eventCount = categoryFeatures.Distribution.GetEventCount(feature);
                    var featureFreqLog =
                        _kSmoothFactor.HasValue
                            ? Math.Log(eventCount + _kSmoothFactor.Value) - categoryFeatures.TotalRepresentedEventCountWithKSmoothingLog.Value
                            : eventCount > 0
                                  ? Math.Log(eventCount) - categoryFeatures.TotalEventCountWithNoiseLog
                                  : categoryFeatures.AverageNoiseFeatureFrequencyLog;
                    featureFreqSum += featureFreqLog;
                }
                result.Add(Tuple.Create(category, featureFreqSum));
            }
            return result.OrderByDescending(t => t.Item2);
        }

        private class DistributionAndMinEventCount<TFeature>
        {
            public IDistribution<TFeature> Distribution { get; set; }
            public double AverageNoiseFeatureFrequencyLog { get; set; }
            public double? TotalRepresentedEventCountWithKSmoothingLog { get; set; }
            public double TotalEventCountWithNoiseLog { get; set; }
            public double CategoryFrequencyLog { get; set; }
        }
    }
}
