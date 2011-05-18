using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NClassify
{
    public interface IClassifier<T>
    {
        double Classify(T obj);
    }
}
