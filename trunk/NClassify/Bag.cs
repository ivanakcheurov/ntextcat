using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IvanAkcheurov.NClassify
{
    public class Bag<T> : IBag<T>
    {
        private Dictionary<T, long> _store = new Dictionary<T, long>();
        private long _totalCopiesCount;

        public long GetNumberOfCopies(T item)
        {
            long count;
            if (_store.TryGetValue(item, out count))
                return count;
            return 0;
        }

        public IEnumerable<T> DistinctItems
        {
            get { return _store.Keys; }
        }

        public bool Add(T item, long copiesCount)
        {
            long oldCount;
            long newCount = (_store.TryGetValue(item, out oldCount) ? oldCount : 0) + copiesCount;
            if (newCount < 0)
                return false;
            _store[item] = newCount;
            _totalCopiesCount += copiesCount;
            return true;
        }

        public bool RemoveCopies(T item, long count)
        {
            return Add(item, -count);
        }

        public void RemoveAllCopies(T item)
        {
            long oldCount;
            if (_store.TryGetValue(item, out oldCount))
            {
                _totalCopiesCount -= oldCount;
                _store.Remove(item);
            }
        }

        public long TotalCopiesCount
        {
            get { return _totalCopiesCount; }
        }

        public IEnumerator<KeyValuePair<T, long>> GetEnumerator()
        {
            return _store.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
