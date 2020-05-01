using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NTextCat
{
    [DebuggerDisplay("ISO639-2-T: {Iso639_2T}, ISO639-3: {Iso639_3}, EnglishName: {EnglishName}, LocalName: {LocalName}")]
    public class LanguageInfo
    {
        /// <summary>
        /// A code of the language according to ISO639-2 (Part2T)
        /// </summary>
        public string Iso639_2T { get; private set; }
        public string Iso639_3 { get; private set; }
        public string EnglishName { get; private set; }
        public string LocalName { get; private set; }

        public LanguageInfo(string iso6392T, string iso6393, string englishName, string localName)
        {
            Iso639_2T = iso6392T;
            Iso639_3 = iso6393;
            EnglishName = englishName;
            LocalName = localName;
        }
    }
}
