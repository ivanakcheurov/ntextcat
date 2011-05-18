using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NClassify;
using NGram = System.UInt64;

namespace NTextCatLib
{
    public class LanguageModelCreator<T>
    {
        public static IDistribution<T> CreateLangaugeModel(IEnumerable<T> tokens, int minOccuranceNumberThreshold, int maxTokensInDistribution)
        {
            IModifiableDistribution<T> distribution = new Distribution<T>();
            distribution.AddEventRange(tokens);
            // text_cat prunes by count, then by rank.
            // resulting distribution should not contain threshold-values (text_cat excludes them),
            // but distribution's PruneByCount leaves threshold in distribution, hence lower threshold by one.
            if (minOccuranceNumberThreshold > 0)
                distribution.PruneByCount(minOccuranceNumberThreshold - 1);
            distribution.PruneByRank(maxTokensInDistribution);
            return distribution;
        }
    }
}
