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
    public class LanguageIdentificationTest
    {
        [Test]
        public void Test()
        {
            var factory =
                new NaiveBayesLanguageIdentifierFactory
                //new RankedLanguageIdentifierFactory
                    {
                        MaxNGramLength = 5,
                        MaximumSizeOfDistribution = 4000,
                        OccuranceNumberThreshold = 0,
                        OnlyReadFirstNLines = Int32.MaxValue
                    };
            using (var stream = File.OpenRead(@"d:\WikiDump\Wiki\extract\trainData20M\profile4000\profile.xml"))
            {
                var identifier = factory.Load(stream);
                var res = identifier.Identify("был зачитан вслух");
                var best = res.First().Item1.Iso639_2;
            }
        }
    }
}
