using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NTextCat.NClassify
{
    public interface ITextReaderTokenizer : IFeatureExtractor<TextReader, string>
    {
    }
}
