using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace NTextCat.Core
{
    public class XmlProfilePersister
    {
        private const string MaximumSizeOfDistributionElement = "MaximumSizeOfDistribution";
        private const string MaxNGramLengthElement = "MaxNGramLength";
        private const string LanguageIdentificationProfileElement = "LanguageIdentificationProfile";
        private const string ParametersElement = "Parameters";
        private const string LanguageModelsElement = "LanguageModels";

        public static void Save<T>(IEnumerable<LanguageModel<T>> languageModels, int maximumSizeOfDistribution, int maxNGramLength, Stream outputStream)
        {
            var languageModelsCache = languageModels.ToArray();
            var languageMarks =
            new Dictionary<string, Func<LanguageInfo, string>>
                {
                    {"ISO 639-3", lm => lm.Iso639_3},
                    {"ISO 639-2-T", lm => lm.Iso639_2T},
                    {"English name", lm => lm.EnglishName},
                    {"local name", lm => lm.LocalName},
                    {"any name available", lm => lm.Iso639_3 ?? lm.Iso639_2T ?? lm.EnglishName ?? lm.LocalName},
                };
            var f = 
                languageMarks.ToDictionary(_ => _.Key, _ => languageModelsCache.Select(lm => _.Value(lm.Language)).ToList())
                .FirstOrDefault(_ => _.Value.All(name => name != null));
            var xComment =
                f.Key == null
                    ? new XComment("WARNING! Some of the language model(s) do(es)n't have any language name assigned")
                    : new XComment("Contains models for the following languages (by " + f.Key  + "): " + String.Join(", ", f.Value));

            if (languageModelsCache.Any(lm => lm.Language.Iso639_3 != null))
                ;
            var persister = new XmlLanguageModelPersister<T>();
            var xDoc =
                new XDocument(
                    xComment,
                    new XElement(LanguageIdentificationProfileElement,
                        new XElement(ParametersElement,
                                              new XElement(MaximumSizeOfDistributionElement, maximumSizeOfDistribution),
                                              new XElement(MaxNGramLengthElement, maxNGramLength)),
                                 new XElement(LanguageModelsElement,
                                              languageModelsCache.Select(persister.ToXml)

                                     )));

            var xmlWriter = XmlWriter.Create(outputStream, new XmlWriterSettings {Indent = true});
            xDoc.WriteTo(xmlWriter);
            xmlWriter.Flush();
        }

        public static IEnumerable<LanguageModel<T>> Load<T>(Stream sourceStream, out int maximumSizeOfDistribution, out int maxNGramLength)
        {
            var xDocument = XDocument.Load(sourceStream);
            return Load<T>(xDocument.Root, out maximumSizeOfDistribution, out maxNGramLength);
        }

        public static IEnumerable<LanguageModel<T>> Load<T>(XElement xProfile, out int maximumSizeOfDistribution, out int maxNGramLength)
        {
            if (xProfile.Name != LanguageIdentificationProfileElement)
                throw new ArgumentException("Xml root is not " + LanguageIdentificationProfileElement, nameof(xProfile));

            var xParameters = xProfile.Element(ParametersElement);
            maximumSizeOfDistribution = int.Parse(xParameters.Element(MaximumSizeOfDistributionElement).Value);
            maxNGramLength = int.Parse(xParameters.Element(MaxNGramLengthElement).Value);
            var xLanguageModels = xProfile.Element(LanguageModelsElement);
            var persister = new XmlLanguageModelPersister<T>();
            return xLanguageModels.Elements(XmlLanguageModelPersister<T>.RootElement).Select(persister.Load).ToList();

        }
    }
}
