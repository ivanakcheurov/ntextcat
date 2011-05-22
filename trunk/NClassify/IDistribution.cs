using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IvanAkcheurov.NClassify
{
    public interface IDistribution<T> : IEnumerable<KeyValuePair<T, double>>
    {
        double this[T obj] { get; }
        IEnumerable<T> Events { get; }
        double GetEventFrequency(T obj);
        long GetEventCount(T obj);
        long TotalEventCount { get; }
    }
}
