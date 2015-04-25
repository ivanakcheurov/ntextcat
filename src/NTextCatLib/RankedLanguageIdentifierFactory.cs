using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IvanAkcheurov.NTextCat.Lib
{
    /// <summary>
    /// Loads an instance of <see cref="RankedLanguageIdentifier"/> from file or trains a new instance out of a data set.
    /// </summary>
    public class RankedLanguageIdentifierFactory : BasicProfileFactoryBase<RankedLanguageIdentifier>
    {
        
        public RankedLanguageIdentifierFactory()
        {
        }

        public RankedLanguageIdentifierFactory(int maxNGramLength, int maximumSizeOfDistribution, int occuranceNumberThreshold, int onlyReadFirstNLines, bool allowMultithreading)
            : base(maxNGramLength, maximumSizeOfDistribution, occuranceNumberThreshold, onlyReadFirstNLines, allowMultithreading)
        {
        }

        public override RankedLanguageIdentifier Create(IEnumerable<LanguageModel<string>> languageModels, int maxNGramLength, int maximumSizeOfDistribution, int occuranceNumberThreshold, int onlyReadFirstNLines)
        {
            var result = new RankedLanguageIdentifier(languageModels, maxNGramLength, maximumSizeOfDistribution, occuranceNumberThreshold, onlyReadFirstNLines);
            return result;
        }

        
    }
}
