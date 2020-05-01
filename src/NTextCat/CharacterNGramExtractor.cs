using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NTextCat.NClassify;

namespace NTextCat
{
    /// <summary>
    /// Extracts char-ngrams out of TextReader, char[] or string.
    /// </summary>
    public class CharacterNGramExtractor : IFeatureExtractor<TextReader, string>, IFeatureExtractor<char[], string>, IFeatureExtractor<string, string> 
    {
        private readonly int _maxNGramLength = 5;
        private readonly long _maxLinesToRead;

        public CharacterNGramExtractor(int maxNGramLength, long maxLinesToRead = long.MaxValue)
        {
            if (maxNGramLength <= 0)
                throw new ArgumentOutOfRangeException("maxNGramLength", "should be positive integer number");
            _maxNGramLength = maxNGramLength;
            _maxLinesToRead = maxLinesToRead;
        }

        /// <summary>
        /// Splits text into tokens, transforms each "token" into "_token_" (prepends and appends underscores) 
        /// and then extracts proper ngrams out of each "_token_".
        /// </summary>
        /// <param name="text"></param>
        /// <returns>the sequence of ngrams extracted</returns>
        public IEnumerable<string> GetFeatures(string text)
        {
            return GetFeatures(new StringReader(text));
        }

        /// <summary>
        /// Splits text into tokens, transforms each "token" into "_token_" (prepends and appends underscores) 
        /// and then extracts proper ngrams out of each "_token_".
        /// </summary>
        /// <param name="text"></param>
        /// <returns>the sequence of ngrams extracted</returns>
        public IEnumerable<string> GetFeatures(char[] text)
        {
            return GetFeatures(new string(text));
        }

        /// <summary>
        /// Splits text into tokens, transforms each "token" into "_token_" (prepends and appends underscores) 
        /// and then extracts proper ngrams out of each "_token_".
        /// </summary>
        /// <param name="text"></param>
        /// <returns>the sequence of ngrams extracted</returns>
        public IEnumerable<string> GetFeatures(TextReader text)
        {
            long numberOfLinesRead = 0;
            var currentNgrams = Enumerable.Range(0, _maxNGramLength).Select(_ => new Queue<char>()).ToArray();
            bool insideWord = false;
            var charsToProcess = new Queue<char>();
            var buffer = new char[4096];
            int charsRead;
            char previousByte = (char)0;
            while ((charsRead = text.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < charsRead; i++)
                {
                    // here we have explicitly implemented transforming "abcdefg" into "_abcdefg_" and getting <1.._maxNGramLength>grams
                    char currentByte = buffer[i];
                    if (currentByte == 0xD || currentByte == 0xA && previousByte != 0xD)
                        numberOfLinesRead++;
                    if (numberOfLinesRead >= _maxLinesToRead)
                        break;

                    bool cleanNgrams = false;

                    if (insideWord)
                    {
                        if (IsSeparator(currentByte))
                        {
                            insideWord = false;
                            charsToProcess.Enqueue('_');
                            cleanNgrams = true;
                        }
                        else
                        {
                            charsToProcess.Enqueue(currentByte);
                        }

                    }
                    else
                    {
                        if (IsSeparator(currentByte))
                        {
                            // skip it;
                        }
                        else
                        {
                            insideWord = true;
                            charsToProcess.Enqueue('_');
                            charsToProcess.Enqueue(currentByte);
                        }
                    }

                    foreach (var ngram in UpdateAndProduceNgrams(charsToProcess, currentNgrams))
                        yield return ngram;

                    if (cleanNgrams)
                    {
                        foreach (Queue<char> ngram in currentNgrams)
                        {
                            ngram.Clear();
                        }
                    }
                    previousByte = currentByte;
                }
            }
            if (insideWord)
            {

                charsToProcess.Enqueue('_');
                foreach (var ngram in UpdateAndProduceNgrams(charsToProcess, currentNgrams))
                    yield return ngram;
            }
        }

        private IEnumerable<string> UpdateAndProduceNgrams(Queue<char> charsToProcess, Queue<char>[] currentNgrams)
        {
            while (charsToProcess.Count > 0)
            {
                var processingByte = charsToProcess.Dequeue();
                for (int j = 0; j < currentNgrams.Length; j++)
                {
                    var currentNgram = currentNgrams[j];
                    // if ngram is complete (e.g. 3gram contains 3 characters)
                    if (currentNgram.Count > j)
                        currentNgram.Dequeue();
                    currentNgram.Enqueue(processingByte);
                    // if ngram is complete (e.g. 3gram contains 3 characters)
                    if (j == 0) // if unigram
                    {
                        var ch = currentNgram.Peek();
                        // prevent pure "_" as ngram otherwise it becomes the most frequent ngram
                        if (ch != '_')
                            yield return new string(ch, 1);
                    }
                    else if (currentNgram.Count > j)
                    {
                        // todo: optimization to remove excessive array creation: Queue => _Array_ => String
                        yield return new string(currentNgram.ToArray());
                    }
                }
            }
        }

        private static bool IsSeparator(char b)
        {
            return Char.IsLetter(b) == false;
        }
    }
}
