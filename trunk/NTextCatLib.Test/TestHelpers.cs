using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NTextCatLib.Test
{
    class TestHelpers
    {
        private const string InputDataFilenameSuffix = ".input";
        private const string ExpectedResultsFilenameSuffix = ".expected";

        public static IEnumerable<TestData> GetTestData(MethodBase methodInfo)
        {
            string methodName = methodInfo.Name;
            string typeName = methodInfo.DeclaringType.Name;
            IEnumerable<TestData> testDataPairs =
                Directory.GetFiles(string.Format("..\\..\\TestData\\{0}.{1}\\", typeName, methodName), "*" + InputDataFilenameSuffix)
                    .Select(i => 
                        new TestData(
                            inputData : File.ReadAllBytes(i),
                            expectedResult: File.ReadAllBytes(i.Remove(i.Length - InputDataFilenameSuffix.Length) + ExpectedResultsFilenameSuffix))
                            );
            return testDataPairs;
        }
        public class TestData
        {
            public TestData(byte[] inputData, byte[] expectedResult)
            {
                InputData = inputData;
                ExpectedResult = expectedResult;
            }

            public byte[] InputData { get; private set; }
            public byte[] ExpectedResult { get; private set; }
        }
    }
}
