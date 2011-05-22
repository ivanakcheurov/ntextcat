using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IvanAkcheurov.NClassify;

namespace IvanAkcheurov.NTextCat.Lib.Legacy
{
    public class ByteToUInt64NGramExtractor : IFeatureExtractor<byte[], ulong>, IFeatureExtractor<Stream, ulong>
    {
        private readonly int _maxNGramLength = 5;
        private readonly long _maxLinesToRead;

        public ByteToUInt64NGramExtractor(int maxNGramLength, long maxLinesToRead = long.MaxValue)
        {
            if (maxNGramLength <= 0)
                throw new ArgumentOutOfRangeException("maxNGramLength", "should be positive integer number");
            _maxNGramLength = maxNGramLength;
            _maxLinesToRead = maxLinesToRead;
        }

        private ulong GetCutMask(int maxNGramLength)
        {
            return ~(0xFFFFFFFFFFFFFFFFul << maxNGramLength * 8);
        }

        public IEnumerable<ulong> GetFeatures(byte[] obj)
        {
            return GetFeatures(new MemoryStream(obj));
        }

        public IEnumerable<ulong> GetFeatures(Stream obj)
        {
            long numberOfLinesRead = 0;
            UInt64[] currentNgrams = new UInt64[_maxNGramLength];
            UInt64[] ngramMasks = Enumerable.Range(1, _maxNGramLength).Select(GetCutMask).ToArray();
            // HACK, insideWord is true to repeat perl text_cat splitting of words: If file starts with space, first word is empty string!
            bool insideWord = true; 
            Queue<byte> bytesToProcess = new Queue<byte>();
            bytesToProcess.Enqueue((byte)'_'); //HACK read above, same as for insideWord
            byte[] buffer = new byte[4096];
            int bytesRead;
            byte previousByte = 0;
            while (( bytesRead = obj.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    // here we have explicitly implemented transforming abcdefg into _abcdefg_ and getting <1.._maxNGramLength>grams
                    byte currentByte = buffer[i];
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
                            bytesToProcess.Enqueue((byte)'_');
                            cleanNgrams = true;
                        }
                        else
                        {
                            bytesToProcess.Enqueue(currentByte);
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
                            bytesToProcess.Enqueue((byte)'_');
                            bytesToProcess.Enqueue(currentByte);
                        }
                    }

                    foreach (var ngram in UpdateAndProduceNgrams(bytesToProcess, currentNgrams, ngramMasks))
                        yield return ngram;
                    
                    if (cleanNgrams)
                    {
                        for (int j = 0; j < currentNgrams.Length; j++)
                        {
                            currentNgrams[j] = 0;
                        }
                    }
                    previousByte = currentByte;
                }
            }
            if (insideWord)
            {
                
                bytesToProcess.Enqueue((byte)'_');
                foreach (var ngram in UpdateAndProduceNgrams(bytesToProcess, currentNgrams, ngramMasks))
                    yield return ngram;
            }
        }

        private IEnumerable<UInt64> UpdateAndProduceNgrams(Queue<byte> bytesToProcess, ulong[] currentNgrams, ulong[] ngramMasks)
        {
            while (bytesToProcess.Count > 0)
            {
                byte processingByte = bytesToProcess.Dequeue();
                for (int j = 0; j < currentNgrams.Length; j++)
                {
                    currentNgrams[j] = ((currentNgrams[j] << 8) + processingByte) & ngramMasks[j];
                    // if ngram is full
                    if (j == 0 || currentNgrams[j] > ngramMasks[j-1])
                        yield return currentNgrams[j];
                }
            }
        }

        private static bool IsSeparator(byte b)
        {
            return b >= '0' && b <= '9' || b == '\r' || b == '\n' || b == '\t' || b == ' ';
        }
    }
}
