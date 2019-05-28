using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NTextCat.NClassify;

namespace NTextCat.Core
{
    public class LanguageModel<T>
    {
        public LanguageInfo Language { get; private set; }

        public IDictionary<string, string> Metadata { get; private set; }
        
        public IDistribution<T> Features { get; private set; }

        public LanguageModel(IDistribution<T> features, LanguageInfo language)
        {
            Language = language;
            Features = features;
            Metadata = new Dictionary<string, string>();
        }

        public LanguageModel(IDistribution<T> features, LanguageInfo language, IDictionary<string, string> metadata)
        {
            Language = language;
            Metadata = metadata;
            Features = features;
        }
    }
}
