using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServerClrIntegration
{
    class IdentifierSettings
    {
        public IdentifierSettings(string embeddedProfilePath, 
            int maxNGramLength, 
            int maximumSizeOfDistribution, 
            int occuranceNumberThreshold, 
            int onlyReadFirstNLines)
        {
            EmbeddedProfilePath = embeddedProfilePath;
            MaxNGramLength = maxNGramLength;
            MaximumSizeOfDistribution = maximumSizeOfDistribution;
            OccuranceNumberThreshold = occuranceNumberThreshold;
            OnlyReadFirstNLines = onlyReadFirstNLines;
        }

        public string EmbeddedProfilePath { get; private set; }
        public int MaxNGramLength { get; private set; }
        public int MaximumSizeOfDistribution { get; private set; }
        public int OccuranceNumberThreshold { get; private set; }
        public int OnlyReadFirstNLines { get; private set; }

        public bool Equals(IdentifierSettings other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.EmbeddedProfilePath, EmbeddedProfilePath) && other.MaxNGramLength == MaxNGramLength && other.MaximumSizeOfDistribution == MaximumSizeOfDistribution && other.OccuranceNumberThreshold == OccuranceNumberThreshold && other.OnlyReadFirstNLines == OnlyReadFirstNLines;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (IdentifierSettings)) return false;
            return Equals((IdentifierSettings) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (EmbeddedProfilePath != null ? EmbeddedProfilePath.GetHashCode() : 0);
                result = (result*397) ^ MaxNGramLength;
                result = (result*397) ^ MaximumSizeOfDistribution;
                result = (result*397) ^ OccuranceNumberThreshold;
                result = (result*397) ^ OnlyReadFirstNLines;
                return result;
            }
        }
    }
}
