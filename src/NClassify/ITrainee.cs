using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IvanAkcheurov.NClassify
{
    public interface ITrainee<T>
    {
        void LearnMatch(T obj);
        void LearnMismatch(T obj);
    }
}
