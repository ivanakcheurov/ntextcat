using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NTextCat.NClassify
{
    /// <summary>
    /// Finds the most probable label (category) for the item that hasn't been seen before.
    /// Given unknown item, finds N the closest known and labeled items. With that information produces the rankings for all labels.
    /// Label's rank is calculated as an average distance to its N closest items.
    /// Current implementation has N set to infinity.
    /// </summary>
    /// <typeparam name="TItem">known items that have been labeled</typeparam>
    /// <typeparam name="TCategory">labels</typeparam>
    public class KnnMonoCategorizedClassifier<TItem, TCategory> : ICategorizedClassifier<TItem, TCategory>
    {
        private IDistanceCalculator<TItem> _distanceCalculator;

        private Dictionary<TItem, TCategory> _knownInstances;

        public KnnMonoCategorizedClassifier(IDistanceCalculator<TItem> distanceCalculator, IDictionary<TItem, TCategory> knownInstances)
        {
            if (distanceCalculator == null) throw new ArgumentNullException(nameof(distanceCalculator));
            if (knownInstances == null) throw new ArgumentNullException(nameof(knownInstances));
            _distanceCalculator = distanceCalculator;
            _knownInstances = new Dictionary<TItem, TCategory>(knownInstances);
            if (_knownInstances.Count == 0)
                throw new ArgumentException("Cannot be empty", nameof(knownInstances));
        }

        public IEnumerable<Tuple<TCategory, double>> Classify(TItem item)
        {
            var categoryDistances = new Dictionary<TCategory, DistanceSumAndCount>();
            foreach (var knownInstance in _knownInstances)
            {
                DistanceSumAndCount distanceSumAndCount;
                if (!categoryDistances.TryGetValue(knownInstance.Value, out distanceSumAndCount))
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
