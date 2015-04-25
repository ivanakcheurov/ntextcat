using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IvanAkcheurov.NClassify
{
    public interface IDistribution<T> : IEnumerable<KeyValuePair<T, double>>, IEnumerable<KeyValuePair<T, long>>
    {
        double this[T obj] { get; }
        
        /// <summary>
        /// Distinct events represented (noise events are not included)
        /// </summary>
        IEnumerable<T> DistinctRepresentedEvents { get; }

        double GetEventFrequency(T obj);
        long GetEventCount(T obj);

        /// <summary>
        /// Total count of distinct events represented (noise events are not included)
        /// </summary>
        long DistinctRepresentedEventsCount { get; }

        /// <summary>
        /// Count of all distinct events including those that have been considered as noise and have no representative
        /// </summary>
        long DistinctEventsCountWithNoise { get; }

        /// <summary>
        /// Count of distinct events that have been considered as noise and have no representative (<see cref="GetEventCount"/> returns 0)
        /// </summary>
        long DistinctNoiseEventsCount { get; }
        
        /// <summary>
        /// Total count of events (including repetitions) that are represented in the distribution (<see cref="GetEventCount"/> returns value &gt; 0)
        /// </summary>
        long TotalRepresentedEventCount { get; }
        
        /// <summary>
        /// Total count of all events (including repetitions) including those that have been considered as noise and have no representative (<see cref="GetEventCount"/> returns 0)
        /// </summary>
        long TotalEventCountWithNoise { get; }

        /// <summary>
        /// Total count of events (including repetitions) that have been considered as noise and have no representative (<see cref="GetEventCount"/> returns 0)
        /// </summary>
        long TotalNoiseEventsCount { get; }
    }
}
