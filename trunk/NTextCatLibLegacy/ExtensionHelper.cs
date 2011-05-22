using System.Collections.Generic;
using System.Linq;
using IvanAkcheurov.NTextCat.Lib;

namespace IvanAkcheurov.NTextCat.Lib.Legacy
{
    public static class ExtensionHelper
    {
        public static IEnumerable<string> NgramsToStrings(this IEnumerable<ulong> ngrams)
        {
            return ngrams.Select(LanguageModelPersister.NgramToString);
        }
    }
}
