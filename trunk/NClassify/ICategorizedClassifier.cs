using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IvanAkcheurov.NClassify
{
    public interface ICategorizedClassifier<TItem, TCategory>
    {
        IEnumerable<Tuple<TCategory, double>> Classify(TItem item);
    }
}
