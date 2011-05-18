using System.Collections.Generic;
using System.Linq;
using NTextCatLib;

namespace NTextCatLibLegacy
{
    public static class ExtensionHelper
    {
        public static IEnumerable<string> NgramsToStrings(this IEnumerable<ulong> ngrams)
        {
            return ngrams.Select(LanguageModelPersister.NgramToString);
        }
    }
}
