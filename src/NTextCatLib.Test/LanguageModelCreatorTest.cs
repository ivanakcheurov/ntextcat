//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using NClassify;
//using NUnit.Framework;

//namespace NTextCatLib.Test
//{
//    [TestFixture]
//    class LanguageModelCreatorTest
//    {
//        [Test]
//        public void TestMaxLength5MaxCount40000()
//        {
//            Test(5, 40000, MethodBase.GetCurrentMethod());
//        }

//        public void Test(int maxNgramLength, int maxNgramCount, MethodBase originalTest)
//        {
//            LanguageModelCreator creator = new LanguageModelCreator(maxNgramLength, maxNgramCount, long.MaxValue, 0);
//            LanguageModelPersister persister = new LanguageModelPersister();
//            VectorDistanceCalculator<ulong> distanceCalculator = new VectorDistanceCalculator<ulong>();
//            var testData = TestHelpers.GetTestData(originalTest);
//            foreach (var data in testData)
//            {
//                IDistribution<ulong> actual = creator.CreateLangaugeModel(data.InputData);
//                IDistribution<ulong> expected = persister.Load(new MemoryStream(data.ExpectedResult));
//                //using (FileStream destinationStream = new FileStream(@"d:\Files\Projects\NLP\NTextCat\NTextCat\NTextCatLib.Test\TestData\LanguageModelCreatorTest.TestMaxLength5MaxCount400\try1.actual", FileMode.Create))
//                //{
//                //    persister.Save(actual, destinationStream);
//                //}
//                double distance = distanceCalculator.CalculateDistance(actual, expected);
//                Assert.IsTrue(distance == 0);
//            }
//        }

//        [Test]
//        public void TestMaxOccuranceThreshold()
//        {
//            string input = "abc abc def gabc";
//            LanguageModelCreator creator = new LanguageModelCreator(5, 40000, long.MaxValue, 1);
//            IDistribution<ulong> distribution = creator.CreateLangaugeModel(Encoding.GetEncoding(1250).GetBytes(input));
//            Assert.IsTrue(distribution.GetEventCount(LanguageModelPersister.StringToNgram("abc")) > 0);
//            Assert.IsTrue(distribution.GetEventCount(LanguageModelPersister.StringToNgram("a")) > 0);
//            Assert.IsTrue(distribution.GetEventCount(LanguageModelPersister.StringToNgram("ab")) > 0);
//            Assert.IsTrue(distribution.GetEventCount(LanguageModelPersister.StringToNgram("bc")) > 0);
//            Assert.IsTrue(distribution.GetEventCount(LanguageModelPersister.StringToNgram("c")) > 0);
//            Assert.IsTrue(distribution.GetEventCount(LanguageModelPersister.StringToNgram("d")) == 0);
//            Assert.IsTrue(distribution.GetEventCount(LanguageModelPersister.StringToNgram("de")) == 0);
//            Assert.IsTrue(distribution.GetEventCount(LanguageModelPersister.StringToNgram("ef")) == 0);
//            Assert.IsTrue(distribution.GetEventCount(LanguageModelPersister.StringToNgram("f")) == 0);
//            Assert.IsTrue(distribution.GetEventCount(LanguageModelPersister.StringToNgram("g")) == 0);
//            Assert.IsTrue(distribution.GetEventCount(LanguageModelPersister.StringToNgram("ga")) == 0);
//            Assert.IsTrue(distribution.GetEventCount(LanguageModelPersister.StringToNgram("gab")) == 0);
//            Assert.IsTrue(distribution.GetEventCount(LanguageModelPersister.StringToNgram("gabc")) == 0);
//        }
//    }
//}
