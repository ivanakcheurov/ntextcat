using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IvanAkcheurov.NTextCat.Lib;
using NUnit.Framework;

namespace IvanAkcheurov.NTextCatLib.Test
{
    [TestFixture]
    public class TokenizerTest
    {
        [Test]
        public void Test()
        {
            var tokenizer = new Tokenizer();
            Assert.That(tokenizer.GetTokens("first second,--214 third"), Is.EquivalentTo(new[] {"first", "second", "third"}));
        }
    }
}
