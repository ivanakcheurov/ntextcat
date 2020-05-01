using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NTextCat.NClassify
{
    public interface IFeatureExtractor<TSource, TFeature>
    {
        IEnumerable<TFeature> GetFeatures(TSource obj);
    }
}
