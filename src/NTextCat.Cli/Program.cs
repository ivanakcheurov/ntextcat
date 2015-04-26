using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using IvanAkcheurov.Commons;
using Mono.Options;
using IvanAkcheurov.NClassify;
using NTextCat;

namespace IvanAkcheurov.NTextCat.Cli
{
    class Program
    {
        private const string NoPromptSwitch = "noprompt";
        static void Main(string[] args)
        {
            //Debugger.Launch();
            //MemoryStream s = new MemoryStream();
            //Console.OpenStandardInput().CopyTo(s);
            double defaultWorstAcceptableThreshold = XmlConvert.ToDouble(ConfigurationManager.AppSettings["WorstAcceptableThreshold"]);
            int defaultTooManyLanguagesThreshold = XmlConvert.ToInt32(ConfigurationManager.AppSettings["TooManyLanguagesThreshold"]);
            string defaultLanguageIdentificationProfileFilePath = ConfigurationManager.AppSettings["LanguageIdentificationProfileFilePath"];
            int defaultOccuranceNumberThreshold = XmlConvert.ToInt32(ConfigurationManager.AppSettings["OccuranceNumberThreshold"]);
            int defaultMaximumSizeOfDistribution = XmlConvert.ToInt32(ConfigurationManager.AppSettings["MaximumSizeOfDistribution"]);
            bool defaultDisallowMultithreading = XmlConvert.ToBoolean(ConfigurationManager.AppSettings["DisallowMultithreading"]);

            bool opt_help = false;
            string opt_train = null;
            Encoding opt_InputEncoding = null;
            string opt_classifyFromArgument = null;
            bool opt_classifyFromInputPerLine = false;
            double opt_WorstAcceptableThreshold = defaultWorstAcceptableThreshold;
            int opt_TooManyLanguagesThreshold = defaultTooManyLanguagesThreshold;
            string opt_LanguageIdentificationProfileFilePath = defaultLanguageIdentificationProfileFilePath;
            int opt_OccuranceNumberThreshold = defaultOccuranceNumberThreshold;
            int opt_OnlyReadFirstNLines = int.MaxValue;
            int opt_MaximumSizeOfDistribution = defaultMaximumSizeOfDistribution;
            bool opt_verbose = false;
            bool opt_disallowMultithreading = defaultDisallowMultithreading;
            bool opt_noPrompt = false;

            int codepage;
            string currentExeName = Path.GetFileName(Environment.GetCommandLineArgs()[0]);

            OptionSet option_set = new OptionSet()

                .Add("?|help|h", "Prints out the options.", option => opt_help = option != null)

                .Add("train=", "Trains from the files specified by VALUE. " + Environment.NewLine +
                               "VALUE can be a wildcard or a directory that contains training files (non-recursive)." + Environment.NewLine +
                                 "Examples:" + Environment.NewLine +
                                 "  " + currentExeName + " -train=SomeDir1" + Environment.NewLine +
                                 "  " + currentExeName + " -train=c:\\temp\\dataset\\*.txt",
                    option =>
                    {
                        opt_train = option;
                    })
                .Add("e=",
                    "indicates which encoding to use to decode the input stream or files into proper text. Ignored when -l option is specified." + Environment.NewLine +
                    "If no encoding is specified, tries to detect the encoding via BOM or uses the standard system's encoding" + Environment.NewLine +
                    "The encoding is specified either via its codepage or its name, e.g. \"UTF-8\"",
                    option => opt_InputEncoding = int.TryParse(option, out codepage) ? Encoding.GetEncoding(codepage) : Encoding.GetEncoding(option))
                .Add("s",
                    @"Determine language of each line of input.",
                    option => opt_classifyFromInputPerLine = option != null)
                .Add("a=",
                    @"the program returns the best-scoring language together" + Environment.NewLine +
                    @"with all languages which are " + defaultWorstAcceptableThreshold + @" times worse (cf option -u). " + Environment.NewLine +
                    @"If the number of languages to be printed is larger than the value " + Environment.NewLine +
                    @"of this option (default: " + defaultTooManyLanguagesThreshold + @") then no language is returned, but" + Environment.NewLine +
                    @"instead a message that the input is of an unknown language is" + Environment.NewLine +
                    @"printed. Default: " + defaultTooManyLanguagesThreshold + @".",
                   (int option) => opt_TooManyLanguagesThreshold = option)
                //.Add("d=",
                //    @"indicates in which directory the language models are" + Environment.NewLine +
                //    @"located (files ending in .lm). Currently only a single" + Environment.NewLine +
                //    @"directory is supported. Default: """ + defaultLanguageModelsDirectory + @""".",
                //   option => opt_LanguageModelsDirectory = option)
                .Add("p=",
                    @"indicates a file from which to load a language identification profile. Default: """ + defaultLanguageIdentificationProfileFilePath + @""".",
                   option => opt_LanguageIdentificationProfileFilePath = option)
                .Add("f=",
                    @"Before sorting is performed the Ngrams which occur this number" + Environment.NewLine +
                    @"of times or less are removed. This can be used to speed up" + Environment.NewLine +
                    @"the program for longer inputs. For short inputs you should use" + Environment.NewLine +
                    @"-f 0." + Environment.NewLine +
                    @"Default: " + defaultOccuranceNumberThreshold + @".",
                   (int option) => opt_OccuranceNumberThreshold = option)
                .Add("i=",
                    @"only read first N lines",
                   (int option) => opt_OnlyReadFirstNLines = option)
                .Add("l=",
                    @"indicates that input is given as an argument on the command line," + Environment.NewLine +
                    @"e.g. text_cat -l ""this is english text""" + Environment.NewLine +
                    @"Cannot be used in combination with -n.",
                   option => opt_classifyFromArgument = option)
                .Add("t=",
                    @"indicates the topmost number of ngrams that should be used." + Environment.NewLine +
                    @"If used in combination with -n this determines the size of the" + Environment.NewLine +
                    @"output. If used with categorization this determines" + Environment.NewLine +
                    @"the number of ngrams that are compared with each of the language" + Environment.NewLine +
                    @"models (but each of those models is used completely)." + Environment.NewLine +
                    @"Default: " + defaultMaximumSizeOfDistribution + @".",
                   (int option) => opt_MaximumSizeOfDistribution = option)
                .Add("u=",
                   @"determines how much worse result must be in order not to be" + Environment.NewLine +
                    "mentioned as an alternative. Typical value: 1.05 or 1.1. " + Environment.NewLine +
                    "Default: " + defaultWorstAcceptableThreshold + @".",
                   (double option) => opt_WorstAcceptableThreshold = option)
                .Add("v",
                   @"verbose. Continuation messages are written to standard error.",
                   option => opt_verbose = option != null)
                .Add("m",
                   @"disallow multithreading. If set to true, training and identification will use a single thread.",
                   option => opt_disallowMultithreading = option != null)
                .Add(NoPromptSwitch,
                   @"prevents text input prompt from being shown.",
                   option => opt_noPrompt = option != null);

            try
            {
                option_set.Parse(args);
            }
            catch (OptionException ex)
            {
                Console.WriteLine("Error occured: " + ex.ToString());
                ShowHelp(option_set);
            }

            if (opt_help)
            {
                ShowHelp(option_set);
                return;
            }

            if (opt_train != null)
            {
                string[] files = null;
                if (Directory.Exists(opt_train))
                {
                    files = Directory.GetFiles(opt_train);
                    if (files.Length == 0)
                        throw new InvalidOperationException("Cannot find files int the following directory: " + opt_train);
                }
                else // treat as a wildcard
                {
                    // avoiding System.ArgumentException: Illegal characters in path.
                    var path = Path.GetDirectoryName(opt_train.Replace('*', '_').Replace('?', '_')) ?? String.Empty;
                    var wildcard = opt_train.Substring(path.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    bool failed = true;
                    if (!string.IsNullOrWhiteSpace(wildcard))
                    {
                        files = Directory.GetFiles(string.IsNullOrWhiteSpace(path) ? "." : path, wildcard, SearchOption.TopDirectoryOnly);
                        if (files.Length > 0)
                            failed = false;
                    }
                    if (failed)
                        throw new InvalidOperationException("Cannot find files with the following wildcard path: " + opt_train);
                }
                var factory = new RankedLanguageIdentifierFactory(5, opt_MaximumSizeOfDistribution, opt_OccuranceNumberThreshold, opt_OnlyReadFirstNLines, !opt_disallowMultithreading);
                var input =
                    files.Select(f =>
                                 Tuple.Create(
                                     new LanguageInfo(Path.GetFileNameWithoutExtension(f), Path.GetFileNameWithoutExtension(f), null, null),
                                     GetTextReader(f, opt_InputEncoding)));
                using (var standardOutput = Console.OpenStandardOutput())
                {
                    var identifier = factory.TrainAndSave(input, standardOutput);
                }
            }
            else
            {
                var factory = new RankedLanguageIdentifierFactory(5, opt_MaximumSizeOfDistribution, opt_OccuranceNumberThreshold, opt_OnlyReadFirstNLines, !opt_disallowMultithreading);
                var languageIdentifier = factory.Load(opt_LanguageIdentificationProfileFilePath);

                if (opt_classifyFromArgument != null)
                {
                    var languages = languageIdentifier.Identify(opt_classifyFromArgument);
                    OutputIdentifiedLanguages(languages, opt_WorstAcceptableThreshold, opt_TooManyLanguagesThreshold);
                }
                else if (opt_classifyFromInputPerLine)
                {
                    if (!opt_noPrompt)
                        DisplayInputPrompt("Classify each line from text input");
                    using (Stream input = Console.OpenStandardInput())
                    using (var reader = GetTextReader(input, opt_InputEncoding))
                    {
                        string line;
                        while((line = reader.ReadLine()) != null)
                        {
                            var languages = languageIdentifier.Identify(line);
                            OutputIdentifiedLanguages(languages, opt_WorstAcceptableThreshold, opt_TooManyLanguagesThreshold);
                        }
                    }
                }
                else // classify all from input
                {
                    if (!opt_noPrompt)
                        DisplayInputPrompt("Classify text input");
                    using (var input = Console.OpenStandardInput())
                    using (var reader = GetTextReader(input, opt_InputEncoding))
                    {
                        var text = reader.ReadToEnd();
                        var languages = languageIdentifier.Identify(text);
                        OutputIdentifiedLanguages(languages, opt_WorstAcceptableThreshold, opt_TooManyLanguagesThreshold);
                    }
                }
            }
        }

        private static void DisplayInputPrompt(string mode)
        {
            Console.WriteLine(
                    string.Empty
                    .AddLine("Welcome!")
                    .AddLine("NTextCat is text classification tool")
                    .AddLine("which is primarily used for identifying language of text.")
                    .AddLine("Current mode is " + mode)
                    .AddLine("To get help on command line switches please type:")
                    .AddLine("\t" + Assembly.GetExecutingAssembly().GetName().Name + " /?")
                    .AddLine("To prevent this prompt from being shown please add /" + NoPromptSwitch + " switch to your command line call.")
                    .AddLine()
                    .AddLine("Currently TEXT INPUT IS EXPECTED from you.")
                    .AddLine("Finish your typing with pressing Enter, Ctrl+Z, Enter.")
                    .ToString()
                    );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="encoding">can be null</param>
        /// <returns></returns>
        private static TextReader GetTextReader(string fileName, Encoding encoding)
        {
            if (encoding == null)
                return new StreamReader(fileName, true);
            return new StreamReader(fileName, encoding);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encoding">can be null</param>
        /// <returns></returns>
        private static TextReader GetTextReader(Stream input, Encoding encoding)
        {
            if (encoding == null)
                return new StreamReader(input, true);
            return new StreamReader(input, encoding);
        }

        private static void OutputIdentifiedLanguages(IEnumerable<Tuple<LanguageInfo, double>> languages, double worstAcceptableThreshold, int tooManyLanguagesThreshold)
        {
            double leastDistance = languages.First().Item2;
            List<Tuple<LanguageInfo, double>> acceptableResults =
                languages.Where(t => t.Item2 <= leastDistance * worstAcceptableThreshold).ToList();
            if (acceptableResults.Count == 0 || acceptableResults.Count > tooManyLanguagesThreshold)
                acceptableResults.Clear();


            if (acceptableResults.Any() == false)
                Console.WriteLine("unknown");
            else
            {
                foreach (var language in acceptableResults)
                {
                    Console.WriteLine(language.Item1.Iso639_3 ?? language.Item1.Iso639_2T ?? language.Item1.EnglishName ?? language.Item1.LocalName);
                }
            }
        }



        private static void ShowHelp(OptionSet optionSet)
        {

            Console.Write(
@"Text Categorization. Typically used to determine the language of a
given document. 

Usage
-----

* print help message:

{0} -h

* for guessing: 

{0} [-a Int] [-d Dir] [-f Int] [-i N] [-l] [-t Int] [-u Int] [-v]

* for creating new language model, based on text read from file or standard input (if filename is not specified):

{0} -n[=filename] [-v]
".FormatWith(Assembly.GetExecutingAssembly().GetName().Name)
            .AddLine()
            .AddLine("Options:"));

            optionSet.WriteOptionDescriptions(Console.Out);
        }
    }
}
