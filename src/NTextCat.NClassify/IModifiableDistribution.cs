using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NTextCat.NClassify
{
    public interface IModifiableDistribution<T> : IDistribution<T>
    {
        void AddEvent(T obj);
        void AddEvent(T obj, long count);

        /// <summary>
        /// Adds noise to the distribution (unrepresented items that effect frequency and TotalCount values)
        /// </summary>
        /// <param name="totalCount">total count of events (including repetitions) that are considered as noise</param>
        /// <param name="distinctCount">count of distinct events that are considered as noise and have not been seen by this distribution ever before 
        /// (it's a burden of a client of the function to guarantee this)</param>
        void AddNoise(long totalCount, long distinctCount);
        void AddEventRange(IEnumerable<T> collection);
        void PruneByRank(long maxRankAllowed);
        void PruneByCount(long minCountAllowed);
    }
}
