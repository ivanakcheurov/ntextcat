using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NTextCat.NClassify
{
    public interface IDistanceCalculator<in T>
    {
        double CalculateDistance(T obj1, T obj2);
    }
}
