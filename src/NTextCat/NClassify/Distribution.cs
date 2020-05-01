using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NTextCat.NClassify
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Implementation of <see cref="IDistribution{T}"/> is not strict as <see cref="Bag{T}"/> cannot contain more than int.MaxValue numbers.
    /// </remarks>
    public class Distribution<T> : IModifiableDistribution<T>
    {
        private IBag<T> _store;
        private bool _containsUnrepresentedNoiseEvents;
        private long _distinctEventsCountWithNoise;
        private long _totalEventCountWithNoise;

        public Distribution(IBag<T> store)
        {
            _store = store;
        }

        #region Implementation of IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IEnumerable<out KeyValuePair<T,double>>

        public IEnumerator<KeyValuePair<T, double>> GetEnumerator()
        {
            throw new NotImplementedException("Maybe a bug, should be divided by _totalEventCountWithNoise");
            long count = _store.TotalCopiesCount;
            return _store.Select(kvp => new KeyValuePair<T, double>(kvp.Key, kvp.Value / (double)count)).GetEnumerator();
        }

        #endregion

        #region Implementation of IEnumerable<out KeyValuePair<T,long>>

        IEnumerator<KeyValuePair<T, long>> IEnumerable<KeyValuePair<T, long>>.GetEnumerator()
        {
            return _store.GetEnumerator();
        }

        #endregion

        #region Implementation of IDistribution<T>

        public double this[T obj]
        {
            get { return GetEventFrequency(obj); }
        }

        public IEnumerable<T> DistinctRepresentedEvents
        {
            get { return _store.DistinctItems; }
        }

        public long DistinctRepresentedEventsCount
        {
            get { return _store.DistinctItemsCount; } 
        }

        public long DistinctEventsCountWithNoise
        {
            get { return _distinctEventsCountWithNoise; }
        }

        public long DistinctNoiseEventsCount
        {
            get { return _distinctEventsCountWithNoise - _store.DistinctItemsCount; }
        }

        public double GetEventFrequency(T obj)
        {
            return _store.GetNumberOfCopies(obj) / (double)_store.TotalCopiesCount;
        }

        public long GetEventCount(T obj)
        {
            return _store.GetNumberOfCopies(obj);
        }

        public long TotalRepresentedEventCount
        {
            get { return _store.TotalCopiesCount; }
        }

        public long TotalEventCountWithNoise
        {
            get { return _totalEventCountWithNoise; }
        }

        public long TotalNoiseEventsCount
        {
            get { return TotalEventCountWithNoise - TotalRepresentedEventCount; }
        }

        #endregion

        #region Implementation of IModifiableDistribution<T>

        public void AddEvent(T obj)
        {
            AddEvent(obj, 1);
        }

        public void AddEvent(T obj, long count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("Cannot add negative number of items");
            _store.Add(obj, count);
            // impossible because the internal bag should contain all the events (including those that will be considered as noise after pruning)
            // otherwise we just cannot reliably keep track _distinctEventsCountWithNoise 
            // (because we cannot distinguish between if the feature has been seen as noise or hasn't been seen at all, 
            // hence do not know if we should add +1 to _distinctEventsCountWithNoise).
            if (_containsUnrepresentedNoiseEvents)
                throw new InvalidOperationException("Cannot add new items to the distribution after it has been pruned.");
            _distinctEventsCountWithNoise = _store.DistinctItemsCount;
            _totalEventCountWithNoise += count;
        }

        public void AddNoise(long totalCount, long distinctCount)
        {
            if (totalCount < 0)
                throw new ArgumentOutOfRangeException("Cannot add negative number of items");
            _containsUnrepresentedNoiseEvents = true;
            _distinctEventsCountWithNoise += distinctCount;
            _totalEventCountWithNoise += totalCount;
        }

        public void AddEventRange(IEnumerable<T> collection)
        {
            foreach (var item in collection)
                AddEvent(item);
        }

        public void PruneByRank(long maxRankAllowed)
        {
            IEnumerable<T> removedItems = 
                _store
                .OrderBy(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .Take((int) Math.Max(0, this.DistinctRepresentedEvents.LongCount() - maxRankAllowed));
            foreach (var item in removedItems)
            {
                _store.RemoveAllCopies(item);
            }
            _containsUnrepresentedNoiseEvents = true;
        }

        public void PruneByCount(long minCountAllowed)
        {
            if (minCountAllowed < 0)
                throw new ArgumentOutOfRangeException("minCountAllowed", "Only non-negative values allowed");
            IEnumerable<T> removedItems =
                _store
                .Where(kvp => kvp.Value < minCountAllowed)
                .Select(kvp => kvp.Key)
                .ToList();
            foreach (var item in removedItems)
            {
                _store.RemoveAllCopies(item);
            }
            _containsUnrepresentedNoiseEvents = true;
        }

        #endregion
    }
}
