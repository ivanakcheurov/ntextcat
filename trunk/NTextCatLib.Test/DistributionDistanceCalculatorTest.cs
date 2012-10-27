using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IvanAkcheurov.NClassify;
using IvanAkcheurov.NTextCat.Lib;
using NUnit.Framework;
using Accord.Statistics.Analysis;

namespace IvanAkcheurov.NTextCatLib.Test
{
    [TestFixture]
    public class DistributionDistanceCalculatorTest
    {
        [Test]
        public void Test()
        {
            var distanceCalculator = new DistributionDistanceCalculator();
            Assert.That(
                distanceCalculator.CalculateDistance(
                    new Distribution<string>(new Bag<string> {{"1", 1}, {"2", 1}, {"3", 1}}),
                    new Distribution<string>(new Bag<string> { { "A", 1 }, { "B", 1 }, { "C", 1 } })), Is.EqualTo(1.0).Within(0.000000000000001));

            Assert.That(
                distanceCalculator.CalculateDistance(
                    new Distribution<string>(new Bag<string> { { "1", 1 }, { "2", 1 }, { "3", 1 } }),
                    new Distribution<string>(new Bag<string> { { "1", 1 }, { "2", 1 }, { "3", 1 } })), Is.EqualTo(0.0));

            Assert.That(
                distanceCalculator.CalculateDistance(
                    new Distribution<string>(new Bag<string> {{"1", 1}, {"2", 2}}),
                    new Distribution<string>(new Bag<string> {{"2", 3}, {"4", 5}})),
                Is.EqualTo(0.625).Within(0.000000000000001));

            Assert.That(
                distanceCalculator.CalculateDistance(
                    new Distribution<string>(new Bag<string> { { "1", 1 }, { "2", 5 }, { "3", 2 } }),
                    new Distribution<string>(new Bag<string> { { "1", 1 }, { "2", 5 }, { "4", 2 } })), 
                Is.EqualTo(0.25).Within(0.000000000000001));
        }
    }
}
