using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace IvanAkcheurov.NTextCat.Lib.Legacy.Test
{
    [TestFixture]
    public class CharacterNgramExtractorTest
    {
        [Test]
        public void Test()
        {
            //using (FileStream stream = new FileStream("TestData\\sample.txt", FileMode.Open))
            var ngrams = new HashSet<string>(new CharacterNGramExtractor(5).GetFeatures("The quick brown fox"));
            Assert.IsTrue(ngrams.SetEquals(expectedNgrams));
        }

        private readonly string[] expectedNgrams =
            new[]
                {
                    "T", "_T", "h", "Th", "_Th", "e", "he", "The", "_The", "e_", "he_", "The_", "_The_",
                    "q", "_q", "u", "qu", "_qu", "i", "ui", "qui", "_qui", "c", "ic", "uic", "quic", "_quic", "k", "ck", "ick", "uick", "quick", "k_", "ck_", "ick_", "uick_",
                    "b", "_b", "r", "br", "_br", "o", "ro", "bro", "_bro", "w", "ow", "row", "brow", "_brow", "n", "wn", "own", "rown", "brown", "n_", "wn_", "own_", "rown_",
                    "f", "_f", "o", "fo", "_fo", "x", "ox", "fox", "_fox", "x_", "ox_", "fox_", "_fox_"
                };

        [Test]
        public void TestMaxLinesToRead()
        {
            //using (FileStream stream = new FileStream("TestData\\sample.txt", FileMode.Open))
            var ngrams = new HashSet<string>(new CharacterNGramExtractor(1, 5).GetFeatures("abcdef\rghjjk\nlmn\nopq\r\nrstu\r\nvwxyz"));
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
