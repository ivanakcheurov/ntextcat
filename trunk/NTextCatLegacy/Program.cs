using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using Mono.Options;
using IvanAkcheurov.NClassify;
using IvanAkcheurov.NTextCat.Lib;
using IvanAkcheurov.NTextCat.Lib.Legacy;

namespace IvanAkcheurov.NTextCat.App.Legacy
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
            string defaultLanguageModelsDirectory = ConfigurationManager.AppSettings["LanguageModelsDirectory"];
            int defaultOccuranceNumberThreshold = XmlConvert.ToInt32(ConfigurationManager.AppSettings["OccuranceNumberThreshold"]);
            int defaultMaximumSizeOfDistribution = XmlConvert.ToInt32(ConfigurationManager.AppSettings["MaximumSizeOfDistribution"]);

            bool opt_help = false;
            bool opt_train = false;
            string opt_trainOnFile = null;
            string opt_classifyFromArgument = null;
            bool opt_classifyFromInputPerLine = false;
            double opt_WorstAcceptableThreshold = defaultWorstAcceptableThreshold;
            int opt_TooManyLanguagesThreshold = defaultTooManyLanguagesThreshold;
            string opt_LanguageModelsDirectory = defaultLanguageModelsDirectory;
            int opt_OccuranceNumberThreshold = defaultOccuranceNumberThreshold;
            long opt_OnlyReadFirstNLines = long.MaxValue;
            int opt_MaximumSizeOfDistribution = defaultMaximumSizeOfDistribution;
            bool opt_verbose = false;
            bool opt_noPrompt = false;
            
            OptionSet option_set = new OptionSet()

                .Add("?|help|h", "Prints out the options.", option => opt_help = option != null)

                .Add("n|train:", "Trains from the file specified or input stream.", 
                    option =>
                                                                      {
                                                                          opt_train = true;
                                                                          opt_trainOnFile = option;
                                                                      })
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
                .Add("d=",
                    @"indicates in which directory the language models are" + Environment.NewLine +
                    @"located (files ending in .lm). Currently only a single" + Environment.NewLine +
                    @"directory is supported. Default: """ + defaultLanguageModelsDirectory + @""".",
                   option => opt_LanguageModelsDirectory = option)
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

            if (opt_train)
            {
                IDistribution<ulong> langaugeModel;
                Stream input;
                if (string.IsNullOrWhiteSpace(opt_trainOnFile))
                {
                    if (!opt_noPrompt) 
                        DisplayInputPrompt("Train from text input");
                    input = Console.OpenStandardInput();
                }
                else input = File.OpenRead(opt_trainOnFile);
                using (input)
                {
                    IEnumerable<UInt64> tokens = new ByteToUInt64NGramExtractor(5, opt_OnlyReadFirstNLines).GetFeatures(input);
                    langaugeModel = LanguageModelCreator<UInt64>.CreateLangaugeModel(tokens, opt_OccuranceNumberThreshold, opt_MaximumSizeOfDistribution);
                }
                using (Stream standardOutput = Console.OpenStandardOutput())
                {
                    new LanguageModelPersister().Save(langaugeModel, standardOutput);
                }
            }
            else
            {
                var languageIdentifier = new LanguageIdentifier(opt_LanguageModelsDirectory, opt_MaximumSizeOfDistribution);
                var settings = new LanguageIdentifier.LanguageIdentifierSettings(
                    opt_TooManyLanguagesThreshold, opt_OccuranceNumberThreshold, opt_OnlyReadFirstNLines,
                    opt_WorstAcceptableThreshold, 5);
                if (opt_classifyFromArgument != null)
                {
                    var languages = languageIdentifier.ClassifyText(opt_classifyFromArgument, settings);
                    OutputIdentifiedLanguages(languages);
                }
                else if (opt_classifyFromInputPerLine)
                {
                    if (!opt_noPrompt) 
                        DisplayInputPrompt("Classify each line from text input");
                    using (Stream input = Console.OpenStandardInput())
                    {
                        // suboptimal read performance, but per-line mode is not intended to be used in heavy scenarios
                        foreach (IEnumerable<byte> line in Split<byte>(EnumerateAllBytes(input), true, 0xD, 0xA))
                        {
                            using (var linestream = new MemoryStream(line.ToArray()))
                            {
                                var languages = languageIdentifier.ClassifyBytes(linestream, null, settings);
                                OutputIdentifiedLanguages(languages);
                            }
                        }
                    }
                }
                else
                {
                    if (!opt_noPrompt) 
                        DisplayInputPrompt("Classify text input");
                    using (Stream input = Console.OpenStandardInput())
                    {
                        var languages = languageIdentifier.ClassifyBytes(input, null, settings);
                        OutputIdentifiedLanguages(languages);
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

        private static IEnumerable<IEnumerable<T>> Split<T>(IEnumerable<T> sequence,  bool removeEmptyEntries, params T[] splitters)
        {
            List<T> buffer = new List<T>();
            foreach (T item in sequence)
            {
                if (splitters.Contains(item))
                {
                    if (buffer.Count > 0)
                        yield return buffer;
                    buffer = new List<T>();
                }
                else
                {
                    buffer.Add(item);
                }
            }
            if (buffer.Count > 0)
                yield return buffer;
        }

        private static IEnumerable<byte> EnumerateAllBytes(Stream stream)
        {
            int readByte;
            while ((readByte = stream.ReadByte()) != -1)
                yield return (byte)readByte;
        }

        private static void OutputIdentifiedLanguages(IEnumerable<Tuple<string, double>> languages)
        {
            languages = languages.ToList();
            if (languages.Any() == false)
                Console.WriteLine("unknown");
            else
            {
                foreach (var language in languages)
                {
                    Console.WriteLine(language.Item1);
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
