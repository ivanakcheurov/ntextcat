using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using IvanAkcheurov.NClassify;

namespace IvanAkcheurov.NTextCat.Lib
{
    public class XmlLanguageModelPersister<T>
    {
        public static readonly string RootElement = "LanguageModel";
        private const string MetadataElement = "metadata";
        private const string NGramsElement = "ngrams";
        private const string NGramElement = "ngram";
        private const string NoiseCountAtribute = "noiseCount";
        private const string TextAttribute = "text";
        private const string CountAttribute = "count";
        private const string LanguageElement = "Language";
        private const string LanguageIso639_2_Attribute = "ISO639-2";
        private const string LanguageIso639_3_Attribute = "ISO639-3";
        private const string LanguageEnglishNameAttribute = "EnglishName";
        private const string LanguageLocalNameAttribute = "LocalName";
        private readonly Func<T, string> _serializeFeature;
        private readonly Func<string, T> _deserializeFeature;

        public XmlLanguageModelPersister()
        {
            _serializeFeature = arg => Convert.ToString(arg, CultureInfo.InvariantCulture);
            _deserializeFeature = text => (T) Convert.ChangeType(text, typeof (T));
        }

        public LanguageModel<T> Load(Stream sourceStream)
        {
            var xDocument = XDocument.Load(sourceStream);
            var result = Load(xDocument.Root);
            return result;
        }

        public LanguageModel<T> Load(XElement xLanguageModel)
        {
            var metadata = xLanguageModel.Element(MetadataElement).Elements().ToDictionary(el => el.Name.ToString(), el => el.Value);
            var xLanguage = xLanguageModel.Element(LanguageElement);
            string iso639_2 = null;
            var xIso639_2 = xLanguage.Attribute(LanguageIso639_2_Attribute);
            if (xIso639_2 != null)
                iso639_2 = xIso639_2.Value;
            string iso639_3 = null;
            var xIso639_3 = xLanguage.Attribute(LanguageIso639_3_Attribute);
            if (xIso639_3 != null)
                iso639_3 = xIso639_3.Value;
            string englishName = null;
            var xEnglishName = xLanguage.Attribute(LanguageEnglishNameAttribute);
            if (xEnglishName != null)
                englishName = xEnglishName.Value;
            string localName = null;
            var xLocalName = xLanguage.Attribute(LanguageLocalNameAttribute);
            if (xLocalName != null)
                localName = xLocalName.Value;
            var language = new LanguageInfo(iso639_2, iso639_3, englishName, localName);
            
            var features = new Distribution<T>(new Bag<T>());
            var xNgramsElement = xLanguageModel.Element(NGramsElement);
            features.AddNoise(long.Parse(xNgramsElement.Attribute(NoiseCountAtribute).Value));
            foreach (var xElement in xNgramsElement.Elements(NGramElement))
            {
                features.AddEvent(_deserializeFeature(xElement.Attribute(TextAttribute).Value), long.Parse(xElement.Attribute(CountAttribute).Value));
            }
            return new LanguageModel<T>(features, language, metadata);
        }

        public void Save(LanguageModel<T> languageModel, Stream destinationStream)
        {
            var document = new XDocument(ToXml(languageModel));
            using (var xmlWriter = XmlWriter.Create(destinationStream, new XmlWriterSettings{Indent = true}))
            {
                document.Save(xmlWriter);
            }
        }

        public XElement ToXml(LanguageModel<T> languageModel)
        {
            var result =
                new XElement(RootElement,
                             new XElement(LanguageElement,
                                          new[]
                                              {
                                                  Tuple.Create(LanguageIso639_2_Attribute, languageModel.Language.Iso639_2),
                                                  Tuple.Create(LanguageIso639_3_Attribute, languageModel.Language.Iso639_3),
                                                  Tuple.Create(LanguageEnglishNameAttribute, languageModel.Language.EnglishName),
                                                  Tuple.Create(LanguageLocalNameAttribute, languageModel.Language.LocalName)
                                              }
                                              .Where(t => t.Item2 != null).Select(t => new XAttribute(t.Item1, t.Item2)).ToArray()),
                             new XElement(MetadataElement,
                                          languageModel.Metadata.Select(kvp => new XElement(kvp.Key, kvp.Value))),
                             new XElement(NGramsElement,
                                          new XAttribute(NoiseCountAtribute, languageModel.Features.TotalNoiseCount),
                                          languageModel.Features.Events
                                              .Select(
                                                  event_ =>
                                                  new XElement(NGramElement,
                                                               new XAttribute(TextAttribute, _serializeFeature(event_)),
                                                               new XAttribute(CountAttribute, languageModel.Features.GetEventCount(event_))))));
            return result;
        }
    }
}
