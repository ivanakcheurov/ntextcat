using System;
using System.Linq;
using Xunit;

namespace NTextCat.Commons.Test
{
    public class TestStringsTextReader
    {
        [Fact]
        public void TestReadToEnd()
        {
            var strings = new[] {"some", "new", "stuff"};
            var stringsTextReader = new StringsTextReader(strings);
            Assert.Equal(strings.Join(Environment.NewLine), stringsTextReader.ReadToEnd());
        }

        [Fact]
        public void TestReadLines()
        {
            var strings = new[] { "some", "new", "stuff" };
            var stringsTextReader = new StringsTextReader(strings);
            Assert.Equal(strings, stringsTextReader.ReadLines().ToArray());
        }

        [Fact]
        public void TestReadBlock()
        {
            var strings = new[] { "some", "new", "stuff" };
            string singleString = strings.Join(Environment.NewLine);
            var stringsTextReader = new StringsTextReader(strings);
            var buff = new char[8];
            Assert.Equal(8, stringsTextReader.ReadBlock(buff, 0, 8));
            Assert.Equal(singleString.Substring(0, 8), new string(buff));
        }

        [Fact]
        public void TestRead()
        {
            var strings = new[] { "some", "new", "stuff" };
            var stringsTextReader = new StringsTextReader(strings);
            foreach (char c in strings.Join(Environment.NewLine))
            {
                Assert.Equal((int)c, stringsTextReader.Read());
            }
            Assert.Equal(-1, stringsTextReader.Read());
            Assert.Equal(-1, stringsTextReader.Read());
            Assert.Null(stringsTextReader.ReadLine());
            Assert.Equal(-1, stringsTextReader.Peek());
        }

        [Fact]
        public void TestReadPeek()
        {
            var strings = new[] { "some", "new", "stuff" };
            var stringsTextReader = new StringsTextReader(strings);
            foreach (char c in strings.Join(Environment.NewLine))
            {
                Assert.Equal((int)c, stringsTextReader.Peek());
                Assert.Equal((int)c, stringsTextReader.Read());
            }
            Assert.Equal(-1, stringsTextReader.Read());
            Assert.Equal(-1, stringsTextReader.Read());
            Assert.Null(stringsTextReader.ReadLine());
            Assert.Equal(-1, stringsTextReader.Peek());
        }
    }
}
