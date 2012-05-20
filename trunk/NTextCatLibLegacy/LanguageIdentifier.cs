using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IvanAkcheurov.Commons.IO;

namespace IvanAkcheurov.NTextCat.Lib.Legacy
{
    public class LanguageIdentifier
    {
        private const int ToManyLanguagesThresholdDefault = 10;
        private const string LanguageModelsDirectoryDefault = @"LM";
        private const int OccuranceNumberThresholdDefault = 0;
        private const long OnlyReadFirstNLinesDefault = long.MaxValue;
        private const int MaximumSizeOfDistributionDefault = 400;
        private const double WorstAcceptableThresholdDefault = 1.05;
        private const int MaxNgramLengthDefault = 5;

        /// <summary>
        /// returns possible languages of text passed or empty sequence if too uncertain
        /// </summary>
        /// <param name="text">text language of which should be identified</param>
        /// <param name="languageModelsDirectory"></param>
        /// <param name="maximumSizeOfDistribution"></param>
        /// <param name="settings">null for default settings</param>
        /// <returns></returns>
        public static IEnumerable<Tuple<string, double>> ClassifyText(
            string text,
            string languageModelsDirectory = LanguageModelsDirectoryDefault,
            int maximumSizeOfDistribution = MaximumSizeOfDistributionDefault,
            LanguageIdentifierSettings settings = null
            )
        {
            var languageIdentifier = new LanguageIdentifier(languageModelsDirectory, maximumSizeOfDistribution);
            return languageIdentifier.ClassifyText(text, settings);
        }

        /// <summary>
        /// returns possible languages of text contained in <paramref name="input"/> or empty sequence if too uncertain.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encoding">encoding of text contained in stream or null if encoding is unknown beforehand.
        /// <para> When encoding is not null, for performance and quality reasons 
        /// make sure that <paramref name="languageModelsDirectory"/> points to models 
        /// built from UTF8 encoded files (Wikipedia-Experimental-UTF8Only)</para></param>
        /// <param name="languageModelsDirectory"></param>
        /// <param name="maximumSizeOfDistribution"></param>
        /// <param name="settings">null for default settings</param>
        /// <returns></returns>
        public static IEnumerable<Tuple<string, double>> ClassifyBytes(
            byte[] input, 
            Encoding encoding = null,
            string languageModelsDirectory = LanguageModelsDirectoryDefault, 
            int maximumSizeOfDistribution = MaximumSizeOfDistributionDefault, 
            LanguageIdentifierSettings settings = null
            )
        {
            var languageIdentifier = new LanguageIdentifier(languageModelsDirectory, maximumSizeOfDistribution);
            return languageIdentifier.ClassifyBytes(input, encoding, settings);
        }

        /// <summary>
        /// returns possible languages of text contained in <paramref name="input"/> or empty sequence if too uncertain.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encoding">encoding of text contained in stream or null if encoding is unknown beforehand.
        /// <para> When encoding is not null, for performance and quality reasons 
        /// make sure that <paramref name="languageModelsDirectory"/> points to models 
        /// built from UTF8 encoded files (Wikipedia-Experimental-UTF8Only)</para></param>
        /// <param name="languageModelsDirectory"></param>
        /// <param name="maximumSizeOfDistribution"></param>
        /// <param name="settings">null for default settings</param>
        /// <returns></returns>
        public static IEnumerable<Tuple<string, double>> ClassifyBytes(
            Stream input, 
            Encoding encoding = null,
            string languageModelsDirectory = LanguageModelsDirectoryDefault, 
            int maximumSizeOfDistribution = MaximumSizeOfDistributionDefault, 
            LanguageIdentifierSettings settings = null
            )
        {
            var languageIdentifier = new LanguageIdentifier(languageModelsDirectory, maximumSizeOfDistribution);
            return languageIdentifier.ClassifyBytes(input, encoding, settings);
        }

        private RankedClassifier<ulong> _classifier;
        private readonly int _maximumSizeOfDistribution;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="languageModelsDirectory"></param>
        /// <param name="maximumSizeOfDistribution"></param>
        public LanguageIdentifier(
            string languageModelsDirectory = LanguageModelsDirectoryDefault, 
            int maximumSizeOfDistribution = MaximumSizeOfDistributionDefault
            )
        {
            _maximumSizeOfDistribution = maximumSizeOfDistribution;
            _classifier = new RankedClassifier<ulong>(_maximumSizeOfDistribution);
            var persister = new LanguageModelPersister();
            foreach (string filename in Directory.GetFiles(languageModelsDirectory))
            {
                using (FileStream sourceStream = File.OpenRead(filename))
                {
                    _classifier.AddEtalonLanguageModel(Path.GetFileNameWithoutExtension(filename), persister.Load(sourceStream));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="languageModelsDirectory"></param>
        /// <param name="maximumSizeOfDistribution"></param>
        public LanguageIdentifier(
            IEnumerable<Tuple<string, Stream>> namesAndLanguageModelStreams,
            int maximumSizeOfDistribution = MaximumSizeOfDistributionDefault
            )
        {
            _maximumSizeOfDistribution = maximumSizeOfDistribution;
            _classifier = new RankedClassifier<ulong>(_maximumSizeOfDistribution);
            var persister = new LanguageModelPersister();
            foreach (var tuple in namesAndLanguageModelStreams)
            {
                using (tuple.Item2)
                {
                    _classifier.AddEtalonLanguageModel(tuple.Item1, persister.Load(tuple.Item2));
                }
            }
        }

        /// <summary>
        /// returns possible languages of text contained in <paramref name="input"/> or empty sequence if too uncertain.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encoding">encoding of text contained in <paramref name="input"/> or null if encoding is unknown beforehand.
        /// <para> When encoding is not null, for performance and quality reasons 
        /// please make sure that <see cref="LanguageIdentifier"/> is created with 
        /// languageModelsDirectory parameter of constructor pointing to models 
        /// built from UTF8 encoded files (models from folder "Wikipedia-Experimental-UTF8Only")</para></param>
        /// <param name="settings">null for default settings</param>
        /// <returns></returns>
        public IEnumerable<Tuple<string, double>> ClassifyBytes(Stream input, Encoding encoding = null, LanguageIdentifierSettings settings = null)
        {
            if (encoding != null && encoding != Encoding.UTF8)
            {
                // we can afford to not dispose TextReaderStream wrapper as it doesn't contain unmanaged resources
                // we do not own base stream passed so we cannot close it
                input = new TextReaderStream(new StreamReader(input, encoding), Encoding.UTF8); // decodes stream into UTF8 from any other encoding
                // todo: restrict to searching among UTF8 language models only
            }
            if (settings == null)
                settings = new LanguageIdentifierSettings();

            IEnumerable<UInt64> tokens = 
                new ByteToUInt64NGramExtractor(settings.MaxNgramLength, settings.OnlyReadFirstNLines)
                .GetFeatures(input);
            var langaugeModel = LanguageModelCreator.CreateLangaugeModel(
                tokens, settings.OccuranceNumberThreshold, _maximumSizeOfDistribution);

            List<Tuple<string, double>> result = _classifier.Classify(langaugeModel).ToList();
            double leastDistance = result.First().Item2;
            List<Tuple<string, double>> acceptableResults = 
                result.Where(t => t.Item2 <= leastDistance * settings.WorstAcceptableThreshold).ToList();
            if (acceptableResults.Count == 0 || acceptableResults.Count > settings.TooManyLanguagesThreshold)
                return Enumerable.Empty<Tuple<string, double>>();
            return acceptableResults;
        }

        /// <summary>
        /// returns possible languages of text encoded in <paramref name="input"/> or empty sequence if too uncertain
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encoding">encoding of text contained in stream or null if encoding is unknown beforehand.
        /// <para> When encoding is not null, for performance and quality reasons 
        /// make sure that <see cref="LanguageIdentifier"/> is created with 
        /// languageModelsDirectory parameter of constructor pointing to models 
        /// built from UTF8 encoded files (models from folder "Wikipedia-Experimental-UTF8Only")</para></param>
        /// <param name="settings">null for default settings</param>
        /// <returns></returns>
        public IEnumerable<Tuple<string, double>> ClassifyBytes(byte[] input, Encoding encoding = null, LanguageIdentifierSettings settings = null)
        {
            using (var stream = new MemoryStream(input))
            {
                return ClassifyBytes(stream, encoding, settings);
            }
        }

        /// <summary>
        /// returns possible languages of text passed or empty sequence if too uncertain
        /// Almost all parameters are optional.
        /// </summary>
        /// <param name="text">text language of which should be identified</param>
        /// <param name="settings">null for default settings</param>
        /// <returns></returns>
        public IEnumerable<Tuple<string, double>> ClassifyText(string text, LanguageIdentifierSettings settings = null)
        {
            return ClassifyBytes(new TextReaderStream(new StringReader(text), Encoding.UTF8), Encoding.UTF8, settings);
        }

        public class LanguageIdentifierSettings
        {
            private readonly int _tooManyLanguagesThreshold;
            private readonly int _occuranceNumberThreshold;
            private readonly long _onlyReadFirstNLines;
            private readonly double _worstAcceptableThreshold;
            private readonly int _maxNgramLength;

            /// <summary>
            /// 
            /// Almost all parameters are optional.
            /// </summary>
            /// <param name="tooManyLanguagesThreshold"></param>
            /// <param name="occuranceNumberThreshold"></param>
            /// <param name="onlyReadFirstNLines"></param>
            /// <param name="worstAcceptableThreshold"></param>
            /// <param name="maxNgramLength"></param>
            /// <returns></returns>
            public LanguageIdentifierSettings(
            int tooManyLanguagesThreshold = ToManyLanguagesThresholdDefault,
            int occuranceNumberThreshold = OccuranceNumberThresholdDefault,
            long onlyReadFirstNLines = OnlyReadFirstNLinesDefault,
            double worstAcceptableThreshold = WorstAcceptableThresholdDefault,
            int maxNgramLength = MaxNgramLengthDefault
            )
            {
                _tooManyLanguagesThreshold = tooManyLanguagesThreshold;
                _occuranceNumberThreshold = occuranceNumberThreshold;
                _onlyReadFirstNLines = onlyReadFirstNLines;
                _worstAcceptableThreshold = worstAcceptableThreshold;
                _maxNgramLength = maxNgramLength;
            }

            public int TooManyLanguagesThreshold
            {
                get { return _tooManyLanguagesThreshold; }
            }

            public int OccuranceNumberThreshold
            {
                get { return _occuranceNumberThreshold; }
            }

            public long OnlyReadFirstNLines
            {
                get { return _onlyReadFirstNLines; }
            }

            public double WorstAcceptableThreshold
            {
                get { return _worstAcceptableThreshold; }
            }

            public int MaxNgramLength
            {
                get { return _maxNgramLength; }
            }
        }


    }
}
