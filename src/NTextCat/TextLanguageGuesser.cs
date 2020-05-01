using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NTextCat.NClassify;

namespace NTextCat
{
    class TextLanguageGuesser : ITrainee<string>, ICategorizedClassifier<string, CultureInfo>
    {
        private KnnMonoCategorizedClassifier<IDistribution<string>, CultureInfo> _classifier;

        public void LearnMatch(string obj)
        {
            throw new NotImplementedException();
        }

        public void LearnMismatch(string obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Tuple<CultureInfo, double>> Classify(string item)
        {

            throw new NotImplementedException();
        }
        
        public void Load(Stream stream)
        {
            
        }

        public void Save(Stream stream)
        {
            
        }
    }
}
