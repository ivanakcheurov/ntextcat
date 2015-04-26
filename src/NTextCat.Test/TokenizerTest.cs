using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NTextCat;
using NUnit.Framework;

namespace NTextCat.Test
{
    [TestFixture]
    public class TokenizerTest
    {
        [Test]
        public void Test()
        {
            var tokenizer = new Tokenizer();
            var inputText = "first second,--214 third";
            var actual = tokenizer.GetTokens(inputText);
            var expected = new[] { "first", "second", "third" };
            Assert.That(actual, Is.EquivalentTo(expected));

        }
    }
}
