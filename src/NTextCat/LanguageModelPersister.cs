using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NTextCat.NClassify;
using NGram = System.UInt64;

namespace NTextCat
{
    public class LanguageModelPersister<T>
    {
        private readonly Func<T, string> _serializeFeature;
        private readonly Func<string, T> _deserializeFeature;
        private readonly Encoding _encoding;

        public LanguageModelPersister()
        {
            _serializeFeature = arg => Convert.ToString(arg, CultureInfo.InvariantCulture);
            _deserializeFeature = text => (T)Convert.ChangeType(text, typeof(T));
            _encoding = Encoding.UTF8;
        }

        protected LanguageModelPersister(Func<T, string> serializeFeature, Func<string, T> deserializeFeature, Encoding encoding)
        {
            _serializeFeature = serializeFeature;
            _deserializeFeature = deserializeFeature;
            _encoding = encoding;
        }

        public LanguageModel<T> Load(Stream sourceStream, LanguageInfo language)
        {
            Distribution<T> result = new Distribution<T>(new Bag<T>());
            StreamReader streamReader = new StreamReader(sourceStream, _encoding);
            
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                string[] keyValue = line.Split(new [] {"\t "}, StringSplitOptions.None);
                if (keyValue.Length != 2)
                    throw new InvalidOperationException("Encountered invalid key value pair in source data.");
                result.AddEvent(_deserializeFeature(keyValue[0]), long.Parse(keyValue[1]));
            }
            return new LanguageModel<T>(result, language);
        }

        public void Save(LanguageModel<T> languageModel, Stream destinationStream)
        {
            var streamWriter = new StreamWriter(destinationStream, _encoding);
            foreach (var keyValuePair in languageModel.Features.OrderByDescending<KeyValuePair<T, long>, long>(kvp => kvp.Value))
            {
                streamWriter.WriteLine("{0}\t {1}", _serializeFeature(keyValuePair.Key), keyValuePair.Value);
            }
            streamWriter.Flush();
        }

        public static string NgramToString(ulong ngram)
        {
            ulong left = ngram;
            var builder = new StringBuilder();
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

    public class ByteLanguageModelPersister : LanguageModelPersister<UInt64>
    {
        public ByteLanguageModelPersister()
            : base(NgramToString, StringToNgram, Encoding.GetEncoding(1250))
        {
        }
    }
}
