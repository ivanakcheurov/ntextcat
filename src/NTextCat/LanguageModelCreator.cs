using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IvanAkcheurov.NClassify;
using NGram = System.UInt64;

namespace NTextCat
{
    public class LanguageModelCreator
    {
        public static IDistribution<ulong> CreateLangaugeModel(IEnumerable<ulong> tokens, int minOccuranceNumberThreshold, int maxTokensInDistribution)
        {
            IModifiableDistribution<ulong> distribution = new Distribution<ulong>(new Bag<ulong>());
            distribution.AddEventRange(tokens);
            // text_cat prunes by count and then by rank.
            // resulting distribution should not contain threshold-values (text_cat excludes them),
            // but distribution's PruneByCount leaves threshold in distribution, hence lower threshold by one.
            // todo: remove correction, update documentation and comments
            if (minOccuranceNumberThreshold > 0)
                distribution.PruneByCount(minOccuranceNumberThreshold - 1);
            distribution.PruneByRank(maxTokensInDistribution);
            return distribution;
        }

        public static IDistribution<T> CreateLangaugeModel<T>(IEnumerable<T> tokens, int minOccuranceNumberThreshold, int maxTokensInDistribution)
        {
            IModifiableDistribution<T> distribution = new Distribution<T>(new Bag<T>());
            distribution.AddEventRange(tokens);
            // text_cat prunes by count and then by rank.
            // resulting distribution should not contain threshold-values (text_cat excludes them),
            // but distribution's PruneByCount leaves threshold in distribution, hence lower threshold by one.
            // todo: remove correction, update documentation and comments
            if (minOccuranceNumberThreshold > 0)
                distribution.PruneByCount(minOccuranceNumberThreshold - 1);
            distribution.PruneByRank(maxTokensInDistribution);
            return distribution;
        }
    }
}
