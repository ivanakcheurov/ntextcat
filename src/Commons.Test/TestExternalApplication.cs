using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace IvanAkcheurov.Commons.IO
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
            Assert.IsTrue(
                StreamLines(ExternalApplication.ToStream("cmd.exe", "/C sort", inputStream))
                    .SequenceEqual(Enumerable.Range(0, 1000000).Select(i => i.ToString()).OrderBy(o => o)));
        }

        [Test]
        public void TestZeroByte()
        {
            byte[] input = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
            string tempFile = null;
            try
            {
                tempFile = Path.GetTempFileName();
                File.WriteAllBytes(tempFile, input);
                using (var stream = new MemoryStream(input))
                using (Stream appStream = ExternalApplication.ToStream("cmd.exe", string.Format("/C type \"{0}\"", tempFile), stream))
                {
                    for (int i = 0; i < 256; i++ )
                        Assert.AreEqual(i, appStream.ReadByte());
                    Assert.AreEqual(-1, appStream.ReadByte());
                }
            }
            finally
            {
                if (tempFile != null)
                    File.Delete(tempFile);
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
