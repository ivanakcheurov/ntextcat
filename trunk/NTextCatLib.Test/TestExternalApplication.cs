using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IvanAkcheurov.NTextCat.Lib;
using NUnit.Framework;

namespace IvanAkcheurov.NTextCatLib.Test
{
    [TestFixture]
    public class TestExternalApplication
    {
        [Test]
        public void Test()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var str in Enumerable.Range(0, 1000000).Reverse().Select(i => i.ToString()))
                builder.AppendLine(str);
            Stream inputStream = new TextReaderStream(new StringReader(builder.ToString()), Encoding.GetEncoding(1250));
            using (var externalApplication = new ExternalApplication("cmd.exe", "/C sort", inputStream))
            {
                Assert.IsTrue(
                    StreamLines(externalApplication.Launch())
                        .SequenceEqual(Enumerable.Range(0, 1000000).Select(i => i.ToString()).OrderBy(o => o)));
            }
        }

        private IEnumerable<string> StreamLines(Stream stream)
        {
            string line;
            var outputReader = new StreamReader(stream);
            while ((line = outputReader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}
