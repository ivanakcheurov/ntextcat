using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace IvanAkcheurov.Commons.Test
{
    [TestFixture]
    class EnumeratorDataReaderTest
    {
        [Test]
        public void TestEnumeratorDataReader()
        {
            var dic =
                new Dictionary<int, string>
                    {
                        {1, "one"},
                        {2, "two"},
                        {3, "three"},
                    };
            using (var dataReader = new EnumeratorDataReader<KeyValuePair<int, string>>(dic.GetEnumerator()))
            {
                Assert.IsTrue(dataReader.Read());
                Assert.That(dataReader.GetValue(0), Is.EqualTo(1));
                Assert.That(dataReader.GetValue(1), Is.EqualTo("one"));
                Assert.IsTrue(dataReader.Read());
                Assert.That(dataReader.GetValue(0), Is.EqualTo(2));
                Assert.That(dataReader.GetValue(1), Is.EqualTo("two"));
                Assert.IsTrue(dataReader.Read());
                Assert.That(dataReader.GetValue(0), Is.EqualTo(3));
                Assert.That(dataReader.GetValue(1), Is.EqualTo("three"));
                Assert.IsFalse(dataReader.Read());
                Assert.IsFalse(dataReader.Read());
            }
        }

        [Test]
        public void TestTupleSequenceDataReader()
        {
            using (var dataReader = TupleSequenceDataReader.Create(new[] { Tuple.Create(1, "7", true), Tuple.Create(2, "8", false) }))
            {
                Assert.IsTrue(dataReader.Read());
                Assert.That(dataReader.GetValue(0), Is.EqualTo(1));
                Assert.That(dataReader.GetValue(1), Is.EqualTo("7"));
                Assert.That(dataReader.GetValue(2), Is.EqualTo(true));
                Assert.IsTrue(dataReader.Read());
                Assert.That(dataReader.GetValue(0), Is.EqualTo(2));
                Assert.That(dataReader.GetValue(1), Is.EqualTo("8"));
                Assert.That(dataReader.GetValue(2), Is.EqualTo(false));
                Assert.IsFalse(dataReader.Read());
                Assert.IsFalse(dataReader.Read());
            }
        }
    }
}
