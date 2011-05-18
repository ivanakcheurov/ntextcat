using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace NTextCatLibLegacy.Test
{
    [TestFixture]
    public class ByteNGramExtractorTest
    {
        [Test]
        public void Test()
        {
            //using (FileStream stream = new FileStream("TestData\\sample.txt", FileMode.Open))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(memoryStream, Encoding.ASCII))
                {
                    writer.Write("The quick brown fox");
                    writer.Flush();
                    HashSet<string> ngrams = new HashSet<string>(new ByteToUInt64NGramExtractor(5).GetFeatures(memoryStream.ToArray()).NgramsToStrings());
                    Assert.IsTrue(ngrams.SetEquals(expectedNgrams));
                }
            }
        }

        private readonly string[] expectedNgrams =
            new[]
                {
                    "_", "T", "_T", "h", "Th", "_Th", "e", "he", "The", "_The", "_", "e_", "he_", "The_", "_The_",
                    "_", "q", "_q", "u", "qu", "_qu", "i", "ui", "qui", "_qui", "c", "ic", "uic", "quic", "_quic", "k", "ck", "ick", "uick", "quick", "_", "k_", "ck_", "ick_", "uick_",
                    "_", "b", "_b", "r", "br", "_br", "o", "ro", "bro", "_bro", "w", "ow", "row", "brow", "_brow", "n", "wn", "own", "rown", "brown", "_", "n_", "wn_", "own_", "rown_",
                    "_", "f", "_f", "o", "fo", "_fo", "x", "ox", "fox", "_fox", "_", "x_", "ox_", "fox_", "_fox_"
                };

        [Test]
        public void TestMaxLinesToRead()
        {
            //using (FileStream stream = new FileStream("TestData\\sample.txt", FileMode.Open))
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(memoryStream, Encoding.ASCII))
                {
                    writer.Write("abcdef\rghjjk\nlmn\nopq\r\nrstu\r\nvwxyz");
                    writer.Flush();
                    HashSet<string> ngrams = new HashSet<string>(new ByteToUInt64NGramExtractor(1, 5).GetFeatures(memoryStream.ToArray()).NgramsToStrings());
                    foreach (char ngram in "abcdefghjjklmnopqrstu")
                    {
                        Assert.IsTrue(ngrams.Contains(ngram.ToString()));
                    }
                    foreach (char ngram in "vwxyz")
                    {
                        Assert.IsFalse(ngrams.Contains(ngram.ToString()));
                    }
                    
                }
            }
        }
    }
}
