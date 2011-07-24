using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IvanAkcheurov.NClassify
{
    public class KnnMonoCategorizedClassifier<TItem, TCategory> : ICategorizedClassifier<TItem, TCategory>
    {
        private IDistanceCalculator<TItem> _distanceCalculator;

        private Dictionary<TItem, TCategory> _knownInstances;

        public KnnMonoCategorizedClassifier(IDistanceCalculator<TItem> distanceCalculator, IDictionary<TItem, TCategory> knownInstances)
        {
            if (distanceCalculator == null) throw new ArgumentNullException("distanceCalculator");
            if (knownInstances == null) throw new ArgumentNullException("knownInstances");
            _distanceCalculator = distanceCalculator;
            _knownInstances = new Dictionary<TItem, TCategory>(knownInstances);
            if (_knownInstances.Count == 0)
                throw new ArgumentException("Cannot be empty", "knownInstances");
        }

        public IEnumerable<Tuple<TCategory, double>> Classify(TItem item)
        {
            var categoryDistances = new Dictionary<TCategory, DistanceSumAndCount>();
            foreach (var knownInstance in _knownInstances)
            {
                DistanceSumAndCount distanceSumAndCount;
                if (categoryDistances.TryGetValue(knownInstance.Value, out distanceSumAndCount) == false)
                {
                    distanceSumAndCount = new DistanceSumAndCount {DistanceSum = 0, Count = 0};
                    categoryDistances.Add(knownInstance.Value, distanceSumAndCount);
                }
                distanceSumAndCount.DistanceSum += _distanceCalculator.CalculateDistance(item, knownInstance.Key);
                distanceSumAndCount.Count++;
            }
            return categoryDistances.Select(t => Tuple.Create(t.Key, t.Value.DistanceSum / t.Value.Count)).OrderBy(t => t.Item2);
        }

        private class DistanceSumAndCount
        {
            public double DistanceSum { get; set; }
            public long Count { get; set; }
        }
    }
}
