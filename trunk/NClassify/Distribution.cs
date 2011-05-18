using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wintellect.PowerCollections;

namespace NClassify
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
        private Bag<T> _distribution = new Bag<T>();

        #region Implementation of IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IEnumerable<out KeyValuePair<T,double>>

        public IEnumerator<KeyValuePair<T, double>> GetEnumerator()
        {
            int count = _distribution.Count;
            return _distribution.DistinctItems().Select(item => new KeyValuePair<T, double>(item, _distribution.NumberOfCopies(item) / (double)count)).GetEnumerator();
        }

        #endregion

        #region Implementation of IDistribution<T>

        public double this[T obj]
        {
            get { return GetEventFrequency(obj); }
        }

        public IEnumerable<T> Events
        {
            get { return _distribution.DistinctItems(); }
        }

        public double GetEventFrequency(T obj)
        {
            return _distribution.NumberOfCopies(obj) / (double)_distribution.Count;
        }

        public long GetEventCount(T obj)
        {
            return _distribution.NumberOfCopies(obj);
        }

        public long TotalEventCount
        {
            get { return _distribution.Count; }
        }

        #endregion

        #region Implementation of IModifiableDistribution<T>

        public void AddEvent(T obj)
        {
            _distribution.Add(obj);
        }

        public void AddEvent(T obj, long count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("Cannot add negative number of items");
            if (count > int.MaxValue)
                throw new NotSupportedException("Current implementation of IDistribution<T> doesn't support more items than 2,147,483,647 of items");
            int numberOfCopies = _distribution.NumberOfCopies(obj);
            _distribution.ChangeNumberOfCopies(obj, numberOfCopies + (int)count);
        }

        public void AddEventRange(IEnumerable<T> collection)
        {
            _distribution.AddMany(collection);
        }

        public void PruneByRank(long maxRankAllowed)
        {
            IEnumerable<T> removedItems = 
                _distribution.DistinctItems()
                .OrderBy(_distribution.NumberOfCopies)
                .Take((int) Math.Max(0, this.Events.LongCount() - maxRankAllowed));
            foreach (var item in removedItems)
            {
                _distribution.RemoveAllCopies(item);
            }
        }

        public void PruneByCount(long minCountAllowed)
        {
            if (minCountAllowed < 0)
                throw new ArgumentOutOfRangeException("minCountAllowed", "Only non-negative values allowed");
            IEnumerable<T> removedItems =
                _distribution.DistinctItems()
                .Where(event_ => _distribution.NumberOfCopies(event_) < minCountAllowed);
            foreach (var item in removedItems)
            {
                _distribution.RemoveAllCopies(item);
            }
        }

        #endregion
    }
}
