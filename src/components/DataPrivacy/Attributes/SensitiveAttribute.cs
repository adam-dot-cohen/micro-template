using System;

namespace Laso.DataPrivacy.Attributes
{
    // TODO: Add Information Type? (e.g. Financial, Credentials, Contact Info, Name, etc.)
    // TODO: Add Rank? (e.g. None, Low, Medium, High, Critical)

    public class SensitiveAttribute : Attribute
    {
        public SensitiveAttribute(SensitivityLevel level = SensitivityLevel.Confidential)
        {
            Level = level;
        }

        public SensitivityLevel Level { get; set; }
    }

    public enum SensitivityLevel
    {
        Public = 1,
        General = 2,
        Confidential = 3,
        Confidential_GDPR = 4, // NOTE: General Data Protection Regulation (GDPR - https://en.wikipedia.org/wiki/General_Data_Protection_Regulation)
        HighlyConfidential = 5,
        HighlyConfidential_GDPR = 6
    }
}
