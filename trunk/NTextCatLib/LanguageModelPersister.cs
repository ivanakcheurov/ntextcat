using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IvanAkcheurov.NClassify;
using NGram = System.UInt64;

namespace IvanAkcheurov.NTextCat.Lib
{
    public class LanguageModelPersister
    {
        public IDistribution<NGram> Load(Stream sourceStream)
        {
            Distribution<NGram> result = new Distribution<ulong>();
            StreamReader streamReader = new StreamReader(sourceStream, Encoding.GetEncoding(1250));
            
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                string[] keyValue = line.Split(new [] {"\t "}, StringSplitOptions.None);
                if (keyValue.Length != 2)
                    throw new InvalidOperationException("Encountered invalid key value pair in source data.");
                result.AddEvent(StringToNgram(keyValue[0]), long.Parse(keyValue[1]));
            }
            return result;
        }

        public void Save(IDistribution<NGram> languageModel, Stream destinationStream)
        {
            StreamWriter streamWriter = new StreamWriter(destinationStream, Encoding.GetEncoding(1250));
            foreach (var keyValuePair in languageModel.OrderByDescending(kvp => kvp.Value))
            {
                streamWriter.WriteLine("{0}\t {1}", NgramToString(keyValuePair.Key), languageModel.GetEventCount(keyValuePair.Key));
            }
            streamWriter.Flush();
        }

        public static string NgramToString(ulong ngram)
        {
            ulong left = ngram;
            StringBuilder builder = new StringBuilder();
            while (left > 0)
            {
                builder.Insert(0, ByteToChar(((byte) (left & 0xFF))));
                left >>= 8;
            }
            return builder.ToString();
        }

        public static NGram StringToNgram(string str)
        {
            ulong ngram = str.Aggregate<char, NGram>(0, (ngr, c) => (ngr << 8) + CharToByte(c));
            return ngram;
        }

        public static byte CharToByte(char c)
        {
            byte[] bytes = Encoding.GetEncoding(1250).GetBytes(new[] { c });
            return bytes.Single();
        }

        public static char ByteToChar(byte b)
        {
            char[] chars = Encoding.GetEncoding(1250).GetChars(new[] { b });
            return chars.Single();
        }
    }
}
