using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IvanAkcheurov.NClassify
{
    public class PowerCollectionsBag<T> : IBag<T>
    {
        public Wintellect.PowerCollections.Bag<T> _store = new Wintellect.PowerCollections.Bag<T>();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<T, long>> GetEnumerator()
        {
            return _store.DistinctItems().Select(item => new KeyValuePair<T, long>(item, _store.NumberOfCopies(item))).GetEnumerator();
        }

        public long GetNumberOfCopies(T item)
        {
            return _store.NumberOfCopies(item);
        }

        public IEnumerable<T> DistinctItems
        {
            get { return _store.DistinctItems(); }
        }

        public bool AddCopies(T item, long count)
        {
            if (count == 1)
            {
                _store.Add(item);
                return true;
            }
            
            if (count > int.MaxValue)
                throw new NotSupportedException("argument count cannot be greater than int.MaxValue");
            int intCount = (int)count;
            int numberOfCopies = _store.NumberOfCopies(item);
            _store.ChangeNumberOfCopies(item, numberOfCopies + intCount);
            return true;
        }

        public bool RemoveCopies(T item, long count)
        {
            return AddCopies(item, -count);
        }

        public void RemoveAllCopies(T item)
        {
            _store.RemoveAllCopies(item);
        }

        public long TotalCopiesCount
        {
            get { return _store.Count; }
        }
    }
}
