using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NClassify
{
    public interface IModifiableDistribution<T> : IDistribution<T>
    {
        void AddEvent(T obj);
        void AddEvent(T obj, long count);
        void AddEventRange(IEnumerable<T> collection);
        void PruneByRank(long maxRankAllowed);
        void PruneByCount(long minCountAllowed);
    }
}
