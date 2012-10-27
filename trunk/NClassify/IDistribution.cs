using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IvanAkcheurov.NClassify
{
    public interface IDistribution<T> : IEnumerable<KeyValuePair<T, double>>
    {
        double this[T obj] { get; }
        
        /// <summary>
        /// Distinct events represented
        /// </summary>
        IEnumerable<T> Events { get; }

        double GetEventFrequency(T obj);
        long GetEventCount(T obj);
        
        /// <summary>
        /// Total count of events that are represented in the distribution (<see cref="GetEventCount"/> returns value &gt; 0)
        /// </summary>
        long TotalRepresentedEventCount { get; }
        
        /// <summary>
        /// Total count of events including those that have been considered as noise and has no representative (<see cref="GetEventCount"/> returns 0)
        /// </summary>
        long TotalEventCountWithNoise { get; }

        /// <summary>
        /// Total count of events that have been considered as noise and has no representative (<see cref="GetEventCount"/> returns 0)
        /// </summary>
        long TotalNoiseCount { get; }
    }
}
