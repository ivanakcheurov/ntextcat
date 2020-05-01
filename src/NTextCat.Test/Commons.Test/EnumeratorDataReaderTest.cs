using System;
using System.Collections.Generic;
using Xunit;

namespace NTextCat.Commons.Test
{
    public class EnumeratorDataReaderTest
    {
        [Fact]
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
                Assert.True(dataReader.Read());
                Assert.Equal(1, dataReader.GetValue(0));
                Assert.Equal("one", dataReader.GetValue(1));
                Assert.True(dataReader.Read());
                Assert.Equal(2, dataReader.GetValue(0));
                Assert.Equal("two", dataReader.GetValue(1));
                Assert.True(dataReader.Read());
                Assert.Equal(3, dataReader.GetValue(0));
                Assert.Equal("three", dataReader.GetValue(1));
                Assert.False(dataReader.Read());
                Assert.False(dataReader.Read());
            }
        }

        [Fact]
        public void TestTupleSequenceDataReader()
        {
            using (var dataReader = TupleSequenceDataReader.Create(new[] { Tuple.Create(1, "7", true), Tuple.Create(2, "8", false) }))
            {
                Assert.True(dataReader.Read());
                Assert.Equal(1, dataReader.GetValue(0));
                Assert.Equal("7", dataReader.GetValue(1));
                Assert.Equal(true, dataReader.GetValue(2));
                Assert.True(dataReader.Read());
                Assert.Equal(2, dataReader.GetValue(0));
                Assert.Equal("8", dataReader.GetValue(1));
                Assert.Equal(false, dataReader.GetValue(2));
                Assert.False(dataReader.Read());
                Assert.False(dataReader.Read());
            }
        }
    }
}
