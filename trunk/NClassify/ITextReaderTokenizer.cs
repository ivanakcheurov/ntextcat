using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NClassify
{
    public interface ITextReaderTokenizer : IFeatureExtractor<TextReader, string>
    {
    }
}
