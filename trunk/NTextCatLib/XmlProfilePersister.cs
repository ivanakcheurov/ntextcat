using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace IvanAkcheurov.NTextCat.Lib
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
            var persister = new XmlLanguageModelPersister<T>();
            var xDoc =
                new XDocument(
                    new XElement(LanguageIdentificationProfileElement,
                                 new XElement(ParametersElement,
                                              new XElement(MaximumSizeOfDistributionElement, maximumSizeOfDistribution),
                                              new XElement(MaxNGramLengthElement, maxNGramLength)),
                                 new XElement(LanguageModelsElement,
                                              languageModels.Select(persister.ToXml)

                                     )));

            var xmlWriter = XmlWriter.Create(outputStream, new XmlWriterSettings {Indent = true});
            xDoc.WriteTo(xmlWriter);
            xmlWriter.Flush();
        }

        public static IEnumerable<LanguageModel<T>> Load<T>(Stream sourceStream, out int maximumSizeOfDistribution, out int maxNGramLength)
        {
            var xDocument = XDocument.Load(sourceStream);
            var result = Load<T>(xDocument.Root, out maximumSizeOfDistribution, out maxNGramLength);
            return result;
        }

        public static IEnumerable<LanguageModel<T>> Load<T>(XElement xProfile, out int maximumSizeOfDistribution, out int maxNGramLength)
        {
            if (xProfile.Name != LanguageIdentificationProfileElement)
                throw new ArgumentException("Xml root is not " + LanguageIdentificationProfileElement, "xProfile");

            var xParameters = xProfile.Element(ParametersElement);
            maximumSizeOfDistribution = int.Parse(xParameters.Element(MaximumSizeOfDistributionElement).Value);
            maxNGramLength = int.Parse(xParameters.Element(MaxNGramLengthElement).Value);
            var xLanguageModels = xProfile.Element(LanguageModelsElement);
            var persister = new XmlLanguageModelPersister<T>();
            var languageModelList =
                xLanguageModels.Elements(XmlLanguageModelPersister<T>.RootElement).Select(persister.Load).ToList();
            return languageModelList;

        }
    }
}
