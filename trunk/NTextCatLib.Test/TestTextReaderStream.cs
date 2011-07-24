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
    public class TestTextReaderStream
    {
        [Test]
        public void TestToUtf8ToString()
        {
            string text =
                @"庐山是中国江西省九江市南郊的一座山，为中国名山之一，也是联合国教科文组织评定的文化遗产和世界地质公园。庐山形成于第四纪冰川时期，是一座地垒式断块山脉，位于鄱阳湖平原的北部、长江的南岸，庐山东及东北为中国最大的淡水湖——鄱阳湖。庐山最高峰为汉阳峰，海拔1474米。庐山景区风景秀丽，气候宜人，夏季气温比山下低得多，为中国知名避暑胜地之一。庐山亦是一座文化名山，被认为是中国山水文化的历史缩影。自东晋以来，中国历代著名的文人、高僧、政客都在此留下过重要的历史印迹，歌咏庐山的诗歌辞赋有4000多首。庐山在中国近现代史上影响非常大，堪称中国的政治名山。1895年起，英、法、美等西方国家曾在此大兴土木，留下了大量的西式建筑，形成了今日牯岭镇的雏形。";
            using (StringReader stringreader = new StringReader(text))
            using (var stream = new TextReaderStream(stringreader, Encoding.UTF8))
            using (var finalReader = new StreamReader(stream, Encoding.UTF8))
            {
                Assert.AreEqual(text, finalReader.ReadToEnd());
            }
        }

        [Test]
        public void TestStringUtf32StringUtf8String()
        {
            string text =
                @"庐山是中国江西省九江市南郊的一座山，为中国名山之一，也是联合国教科文组织评定的文化遗产和世界地质公园。庐山形成于第四纪冰川时期，是一座地垒式断块山脉，位于鄱阳湖平原的北部、长江的南岸，庐山东及东北为中国最大的淡水湖——鄱阳湖。庐山最高峰为汉阳峰，海拔1474米。庐山景区风景秀丽，气候宜人，夏季气温比山下低得多，为中国知名避暑胜地之一。庐山亦是一座文化名山，被认为是中国山水文化的历史缩影。自东晋以来，中国历代著名的文人、高僧、政客都在此留下过重要的历史印迹，歌咏庐山的诗歌辞赋有4000多首。庐山在中国近现代史上影响非常大，堪称中国的政治名山。1895年起，英、法、美等西方国家曾在此大兴土木，留下了大量的西式建筑，形成了今日牯岭镇的雏形。";
            using (StringReader stringreader = new StringReader(text))
            using (var utf32Stream = new TextReaderStream(stringreader, Encoding.UTF32))
            using (var utf32Reader = new StreamReader(utf32Stream, Encoding.UTF32))
            using (var utf8Stream = new TextReaderStream(stringreader, Encoding.UTF8))
            using (var utf8Reader = new StreamReader(utf8Stream, Encoding.UTF8))
            {
                Assert.AreEqual(text, utf8Reader.ReadToEnd());
            }
        }
    }
}
