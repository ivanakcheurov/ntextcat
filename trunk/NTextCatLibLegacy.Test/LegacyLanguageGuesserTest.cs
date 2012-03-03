using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IvanAkcheurov.NClassify;
using IvanAkcheurov.NTextCat.Lib;
using IvanAkcheurov.NTextCat.Lib.Test;
using NUnit.Framework;

namespace IvanAkcheurov.NTextCat.Lib.Legacy.Test
{
    [TestFixture]
    class LegacyLanguageGuesserTest
    {
        [Test]
        [Ignore("Identifies as Bulgarian instead of Russian")]
        public void Test2()
        {
            //"Главная задача сэмпла - предоставить желающим качать возможность оценить реальное качество материала без скачивания всей раздачи целиком. Поэтому вырезать сэмпл надо из середины фильма и без каких либо искажений. Достаточно фрагмента на 1-2 минуты. Заливать сэмпл следует только на файлообменники";
        }

        [Test]
        public void Test()
        {
            byte[] englishEtalon =
                Encoding.GetEncoding(1251).GetBytes(
                @"Many of his novels, with their recurrent concern for social reform, first appeared in magazines in serialised form, 
a popular format at the time. Unlike other authors who completed entire novels before serialisation, Dickens often created the episodes as they were being serialized. 
The practice lent his stories a particular rhythm, punctuated by cliffhangers to keep the public looking forward to the next installment.[2] 
The continuing popularity of his novels and short stories is such that they have never gone out of print.[3]");
            
            byte[] englishQuery =
                Encoding.GetEncoding(1251).GetBytes(
                @"Shakespeare was born and raised in Stratford-upon-Avon. At the age of 18, he married Anne Hathaway, 
with whom he had three children: Susanna, and twins Hamnet and Judith. Between 1585 and 1592, he began a successful career in London as an actor, writer, 
and part owner of a playing company called the Lord Chamberlain's Men, later known as the King's Men. He appears to have retired to Stratford around 1613, 
where he died three years later. Few records of Shakespeare's private life survive, and there has been considerable speculation about such matters as his physical 
appearance, sexuality, religious beliefs, and whether the works attributed to him were written by others.
Shakespeare produced most of his known work between 1589 and 1613.[5][nb 4] His early plays were mainly comedies and histories, genres he 
raised to the peak of sophistication and artistry by the end of the 16th century. He then wrote mainly tragedies until about 1608, including Hamlet, 
King Lear, and Macbeth, considered some of the finest works in the English language. In his last phase, he wrote tragicomedies, also known as romances, 
and collaborated with other playwrights.");
            
            byte[] dutchEtalon =
                Encoding.GetEncoding(1251).GetBytes(
                @"Dickens werd geboren als zoon van John Dickens en Elizabeth Barrow. Toen hij tien was, verhuisde de familie naar Londen. 
Door financiële moeilijkheden van zijn vader (hij werd wegens schulden in de gevangenis gezet) moest de jonge Charles enkele malen zijn school 
verlaten om te gaan werken. Zo belandde hij op twaalfjarige leeftijd in een schoensmeerfabriek waar hij tien uur per dag moest werken. 
De omstandigheden waaronder arbeiders moesten leven werden een belangrijk onderwerp in zijn latere werk.");
            byte[] dutchQuery =
                Encoding.GetEncoding(1251).GetBytes(
                @"William Shakespeare (ook gespeld Shakspere, Shaksper, en Shake-speare, 
omdat de spelling in de Elizabethaanse periode niet absoluut was) werd geboren in Stratford-upon-Avon in Warwickshire, in april 1564. 
William was de zoon van John Shakespeare, een succesvolle handelaar en wethouder, en van Mary Arden, een dochter uit een adellijke familie. 
De Shakespeares woonden toen in Henley Street. Bekend is dat William op 26 april werd gedoopt. Omdat het destijds gebruikelijk was om een kind drie 
dagen na de geboorte te dopen, is Shakespeare waarschijnlijk op zondag 23 april geboren. Het huis in Stratford is bekend als 'de geboorteplaats van Shakespeare,' 
maar deze status is onzeker. Shakespeares vader was een welvarende handschoenenmaker en verkreeg vele titels tijdens zijn leven, met inbegrip van chamberlain,[1] 
wethouder, deurwaarder (equivalent van burgemeester), en eerste schepen. Later werd hij vervolgd voor deelname aan de zwarte markt in wol en verloor zijn positie 
als wethouder. Sommige gegevens wijzen op mogelijke roomse sympathieën aan beide kanten van het gezin - een gevaar onder de strenge anti-katholieke regels van 
koningin Elizabeth.");

            byte[] russianEtalon =
                Encoding.GetEncoding(1251).GetBytes(
                @"Его отец был довольно состоятельным чиновником, человеком весьма легкомысленным, но весёлым и добродушным, 
со вкусом пользовавшимся тем уютом, тем комфортом, которым так дорожила всякая зажиточная семья старой Англии. Своих детей и, 
в частности, своего любимца Чарли, мистер Диккенс окружил заботой и лаской.
Маленький Чарльз унаследовал от отца богатое воображение, лёгкость слова, по-видимому, присоединив к этому некоторую жизненную серьёзность, 
унаследованную от матери, на плечи которой падали все житейские заботы по сохранению благосостояния семьи.
Богатые способности мальчика восхищали родителей, и артистически настроенный отец буквально изводил своего сынишку, заставляя его разыгрывать 
разные сцены, рассказывать свои впечатления, импровизировать, читать стихи и т. д. Диккенс превратился в маленького актёра, преисполненного самовлюблённости и тщеславия.");
            byte[] russianQuery =
                Encoding.GetEncoding(1251).GetBytes(
                @"Считается, что Шекспир учился в стратфордской «грамматической школе» (англ. «grammar school»), 
где получил серьёзное образование: стратфордский учитель латинского языка и словесности писал стихи на латыни. Некоторые 
учёные утверждают, что Шекспир посещал школу короля Эдуарда VI в Стратфорде-на-Эйвоне, где изучал творчество таких поэтов, 
как Овидий и Плавт[6], однако школьные журналы не сохранились[7], и теперь ничего нельзя сказать наверняка.");

            var tokenizer = new ByteToUInt64NGramExtractor(5);
            Func<byte[], IDistribution<UInt64>> languageModelCreator = 
                bytes => LanguageModelCreator.CreateLangaugeModel(tokenizer.GetFeatures(bytes), 0, 400);

            var guesser = new RankedClassifier<ulong>(400);
            guesser.AddEtalonLanguageModel("en", languageModelCreator(englishEtalon));
            guesser.AddEtalonLanguageModel("nl", languageModelCreator(dutchEtalon));
            guesser.AddEtalonLanguageModel("ru", languageModelCreator(russianEtalon));
            Assert.AreEqual("nl", guesser.Classify(languageModelCreator(dutchQuery)).First().Item1);
            Assert.AreEqual("ru", guesser.Classify(languageModelCreator(russianQuery)).First().Item1);
            Assert.AreEqual("en", guesser.Classify(languageModelCreator(englishQuery)).First().Item1);
        }

        [Test]
        [Ignore("Test is not implemented completely")]
        public void ExhaustiveTest()
        {
            IEnumerable<TestHelpers.TestData> testDatas = TestHelpers.GetTestData(System.Reflection.MethodBase.GetCurrentMethod());

        }
    }
}
