using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IvanAkcheurov.NClassify
{
    public interface IBag<T> : IEnumerable<KeyValuePair<T, long>>
    {
        long GetNumberOfCopies(T item);
        IEnumerable<T> DistinctItems { get; }
        bool AddCopies(T item, long count);
        bool RemoveCopies(T item, long count);
        void RemoveAllCopies(T item);
        long TotalCopiesCount { get; }
    }
}
