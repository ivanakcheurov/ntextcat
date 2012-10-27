using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace IvanAkcheurov.NTextCat.Lib
{
    [DebuggerDisplay("ISO639-2: {Iso639_2}, ISO639-3: {Iso639_3}, EnglishName: {EnglishName}, LocalName: {LocalName}")]
    public class LanguageInfo
    {
        public string Iso639_2 { get; private set; }
        public string Iso639_3 { get; private set; }
        public string EnglishName { get; private set; }
        public string LocalName { get; private set; }

        public LanguageInfo(string iso6392, string iso6393, string englishName, string localName)
        {
            Iso639_2 = iso6392;
            Iso639_3 = iso6393;
            EnglishName = englishName;
            LocalName = localName;
        }
    }
}
