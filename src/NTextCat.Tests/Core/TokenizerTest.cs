using NTextCat.Core;
using Xunit;

namespace NTextCat.Test
{

    public class TokenizerTest
    {
        [Fact]
        public void Test()
        {
            var tokenizer = new Tokenizer();
            var inputText = "first second,--214 third";
            var actual = tokenizer.GetTokens(inputText);
            var expected = new[] { "first", "second", "third" };
            Assert.Equal(actual, expected);

        }
    }
}
