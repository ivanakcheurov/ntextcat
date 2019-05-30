using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NTextCat.NClassify;

namespace NTextCat.Core
{
    public class Tokenizer : IFeatureExtractor<TextReader, string>, IFeatureExtractor<char[], string>, IFeatureExtractor<string, string> 
    {
        public int MaxLinesToRead { get; private set; }
        public Func<char, bool> IsSeparatorPredicate { get; private set; }

        public Tokenizer(int maxLinesToRead = int.MaxValue)
            :this(maxLinesToRead, IsSeparator)
        {
        }

        public Tokenizer(int maxLinesToRead, Func<char, bool> isSeparatorPredicate)
        {
            MaxLinesToRead = maxLinesToRead;
            IsSeparatorPredicate = isSeparatorPredicate;
        }

        #region Implementation of IFeatureExtractor<TextReader,string>

        IEnumerable<string> IFeatureExtractor<TextReader, string>.GetFeatures(TextReader obj)
        {
            return GetTokens(obj);
        }

        #endregion

        #region Implementation of IFeatureExtractor<char[],string>

        IEnumerable<string> IFeatureExtractor<char[], string>.GetFeatures(char[] obj)
        {
            return GetTokens(obj);
        }

        #endregion

        #region Implementation of IFeatureExtractor<string,string>

        IEnumerable<string> IFeatureExtractor<string, string>.GetFeatures(string obj)
        {
            return GetTokens(obj);
        }

        #endregion

        public IEnumerable<string> GetTokens(string text)
        {
            return GetTokens(new StringReader(text));
        }

        public IEnumerable<string> GetTokens(char[] text)
        {
            return GetTokens(new string(text));
        }

        public IEnumerable<string> GetTokens(TextReader text)
        {
            long numberOfLinesRead = 0;
            bool insideWord = false;
            var buffer = new char[4096];
            int charsRead;
            char previousByte = (char)0;
            var sb = new StringBuilder();
            while ((charsRead = text.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < charsRead; i++)
                {
                    // here we have explicitly implemented transforming "abcdefg" into "_abcdefg_" and getting <1.._maxNGramLength>grams
                    char currentByte = buffer[i];
                    if (currentByte == 0xD || currentByte == 0xA && previousByte != 0xD)
                        numberOfLinesRead++;
                    if (numberOfLinesRead >= MaxLinesToRead)
                        break;

                    if (insideWord)
                    {
                        if (IsSeparatorPredicate(currentByte))
                        {
                            insideWord = false;
                            yield return sb.ToString();
                            sb.Clear();
                        }
                        else
                        {
                            sb.Append(currentByte);
                        }

                    }
                    else
                    {
                        if (IsSeparatorPredicate(currentByte))
                        {
                            // skip it;
                        }
                        else
                        {
                            insideWord = true;
                            sb.Append(currentByte);
                        }
                    }

                    previousByte = currentByte;
                }
            }
            if (insideWord)
            {
                yield return sb.ToString();
            }
        }

        private static bool IsSeparator(char b)
        {
            return !Char.IsLetter(b);
        }
    }
}
