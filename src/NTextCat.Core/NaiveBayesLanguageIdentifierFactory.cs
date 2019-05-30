using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NTextCat.Core
{
    /// <summary>
    /// Loads an instance of <see cref="NaiveBayesLanguageIdentifier"/> from file or trains a new instance out of a data set.
    /// </summary>
    public class NaiveBayesLanguageIdentifierFactory : BasicProfileFactoryBase<NaiveBayesLanguageIdentifier>
    {
        public NaiveBayesLanguageIdentifierFactory()
        {
        }

        public NaiveBayesLanguageIdentifierFactory(int maxNGramLength, int maximumSizeOfDistribution, int occuranceNumberThreshold, int onlyReadFirstNLines, bool allowMultithreading)
            : base(maxNGramLength, maximumSizeOfDistribution, occuranceNumberThreshold, onlyReadFirstNLines, allowMultithreading)
        {
        }

        public override NaiveBayesLanguageIdentifier Create(IEnumerable<LanguageModel<string>> languageModels, int maxNGramLength, int maximumSizeOfDistribution, int occuranceNumberThreshold, int onlyReadFirstNLines)
        {
            return new NaiveBayesLanguageIdentifier(languageModels, maxNGramLength, onlyReadFirstNLines);
        }
    }
}
