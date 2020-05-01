using NTextCat.NClassify;
using Xunit;

namespace NTextCat.Test
{

    public class DistributionDistanceCalculatorTest
    {
        [Fact]
        public void Test()
        {
            var distanceCalculator = new DistributionDistanceCalculator();
            Assert.Equal(
1.0,                 distanceCalculator.CalculateDistance(
                    new Distribution<string>(new Bag<string> {{"1", 1}, {"2", 1}, {"3", 1}}),
                    new Distribution<string>(new Bag<string> { { "A", 1 }, { "B", 1 }, { "C", 1 } })));

            Assert.Equal(
0.0,                 distanceCalculator.CalculateDistance(
                    new Distribution<string>(new Bag<string> { { "1", 1 }, { "2", 1 }, { "3", 1 } }),
                    new Distribution<string>(new Bag<string> { { "1", 1 }, { "2", 1 }, { "3", 1 } })));

            Assert.Equal(
                0.625,
                distanceCalculator.CalculateDistance(
                    new Distribution<string>(new Bag<string> {{"1", 1}, {"2", 2}}),
                    new Distribution<string>(new Bag<string> {{"2", 3}, {"4", 5}})));

            Assert.Equal(
                0.25, 
                distanceCalculator.CalculateDistance(
                    new Distribution<string>(new Bag<string> { { "1", 1 }, { "2", 5 }, { "3", 2 } }),
                    new Distribution<string>(new Bag<string> { { "1", 1 }, { "2", 5 }, { "4", 2 } })));
        }
    }
}
