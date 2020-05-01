using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace NTextCat.Commons
{
    public class TupleSequenceDataReader
    {
        public static IDataReader Create<T1>(IEnumerable<Tuple<T1>> tuples)
        {
            return new EnumeratorDataReader<Tuple<T1>>(tuples.GetEnumerator(), new[]{ "Item1" });
        }

        public static IDataReader Create<T1, T2>(IEnumerable<Tuple<T1, T2>> tuples)
        {
            return new EnumeratorDataReader<Tuple<T1, T2>>(tuples.GetEnumerator(), new[] { "Item1", "Item2" });
        }

        public static IDataReader Create<T1, T2, T3>(IEnumerable<Tuple<T1, T2, T3>> tuples)
        {
            return new EnumeratorDataReader<Tuple<T1, T2, T3>>(tuples.GetEnumerator(), new[] { "Item1", "Item2", "Item3" });
        }

        public static IDataReader Create<T1, T2, T3, T4>(IEnumerable<Tuple<T1, T2, T3, T4>> tuples)
        {
            return new EnumeratorDataReader<Tuple<T1, T2, T3, T4>>(tuples.GetEnumerator(), new[] { "Item1", "Item2", "Item3", "Item4" });
        }

        public static IDataReader Create<T1, T2, T3, T4, T5>(IEnumerable<Tuple<T1, T2, T3, T4, T5>> tuples)
        {
            return new EnumeratorDataReader<Tuple<T1, T2, T3, T4, T5>>(tuples.GetEnumerator(), new[] { "Item1", "Item2", "Item3", "Item4", "Item5" });
        }

        public static IDataReader Create<T1, T2, T3, T4, T5, T6>(IEnumerable<Tuple<T1, T2, T3, T4, T5, T6>> tuples)
        {
            return new EnumeratorDataReader<Tuple<T1, T2, T3, T4, T5, T6>>(tuples.GetEnumerator(), new[] { "Item1", "Item2", "Item3", "Item4", "Item5", "Item6" });
        }

        public static IDataReader Create<T1, T2, T3, T4, T5, T6, T7>(IEnumerable<Tuple<T1, T2, T3, T4, T5, T6, T7>> tuples)
        {
            return new EnumeratorDataReader<Tuple<T1, T2, T3, T4, T5, T6, T7>>(tuples.GetEnumerator(), new[] { "Item1", "Item2", "Item3", "Item4", "Item5", "Item6", "Item7" });
        }

        public static IDataReader Create<T1, T2, T3, T4, T5, T6, T7, T8>(IEnumerable<Tuple<T1, T2, T3, T4, T5, T6, T7, T8>> tuples)
        {
            return new EnumeratorDataReader<Tuple<T1, T2, T3, T4, T5, T6, T7, T8>>(tuples.GetEnumerator(), new[] { "Item1", "Item2", "Item3", "Item4", "Item5", "Item6", "Item7", "Item8" });
        }
    }
}
