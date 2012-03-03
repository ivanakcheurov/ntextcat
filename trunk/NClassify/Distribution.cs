using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IvanAkcheurov.NClassify
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
            long count = _store.TotalCopiesCount;
            return _store.Select(kvp => new KeyValuePair<T, double>(kvp.Key, kvp.Value / (double)count)).GetEnumerator();
        }

        #endregion

        #region Implementation of IDistribution<T>

        public double this[T obj]
        {
            get { return GetEventFrequency(obj); }
        }

        public IEnumerable<T> Events
        {
            get { return _store.DistinctItems; }
        }

        public double GetEventFrequency(T obj)
        {
            return _store.GetNumberOfCopies(obj) / (double)_store.TotalCopiesCount;
        }

        public long GetEventCount(T obj)
        {
            return _store.GetNumberOfCopies(obj);
        }

        public long TotalEventCount
        {
            get { return _store.TotalCopiesCount; }
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
            _store.AddCopies(obj, count);
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
                .Take((int) Math.Max(0, this.Events.LongCount() - maxRankAllowed));
            foreach (var item in removedItems)
            {
                _store.RemoveAllCopies(item);
            }
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
        }

        #endregion
    }
}
