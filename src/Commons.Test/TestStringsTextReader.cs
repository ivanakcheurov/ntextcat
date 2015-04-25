using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace IvanAkcheurov.Commons.Test
{
    [TestFixture]
    class TestStringsTextReader
    {
        [Test]
        public void TestReadToEnd()
        {
            var strings = new[] {"some", "new", "stuff"};
            var stringsTextReader = new StringsTextReader(strings);
            Assert.AreEqual(strings.Join(Environment.NewLine), stringsTextReader.ReadToEnd());
        }

        [Test]
        public void TestReadLines()
        {
            var strings = new[] { "some", "new", "stuff" };
            var stringsTextReader = new StringsTextReader(strings);
            Assert.AreEqual(strings, stringsTextReader.ReadLines().ToArray());
        }

        [Test]
        public void TestReadBlock()
        {
            var strings = new[] { "some", "new", "stuff" };
            string singleString = strings.Join(Environment.NewLine);
            var stringsTextReader = new StringsTextReader(strings);
            var buff = new char[8];
            Assert.AreEqual(8, stringsTextReader.ReadBlock(buff, 0, 8));
            Assert.AreEqual(singleString.Substring(0, 8), new string(buff));
        }

        public void TestRead()
        {
            var strings = new[] { "some", "new", "stuff" };
            var stringsTextReader = new StringsTextReader(strings);
            foreach (char c in strings.Join(Environment.NewLine))
            {
                Assert.AreEqual((int)c, stringsTextReader.Read());
            }
            Assert.AreEqual(-1, stringsTextReader.Read());
            Assert.AreEqual(-1, stringsTextReader.Read());
            Assert.AreEqual(null, stringsTextReader.ReadLine());
            Assert.AreEqual(-1, stringsTextReader.Peek());
        }

        [Test]
        public void TestReadPeek()
        {
            var strings = new[] { "some", "new", "stuff" };
            var stringsTextReader = new StringsTextReader(strings);
            foreach (char c in strings.Join(Environment.NewLine))
            {
                Assert.AreEqual((int)c, stringsTextReader.Peek());
                Assert.AreEqual((int)c, stringsTextReader.Read());
            }
            Assert.AreEqual(-1, stringsTextReader.Read());
            Assert.AreEqual(-1, stringsTextReader.Read());
            Assert.AreEqual(null, stringsTextReader.ReadLine());
            Assert.AreEqual(-1, stringsTextReader.Peek());
        }
    }
}
