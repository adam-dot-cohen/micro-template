using System;

namespace Partner.Services.DataImport
{
    public enum ImportFrequency
    {
        Daily,
        Weekly,
        Monthly,
        Quarterly,
        Yearly,
        OnRequest
    }

    public static class ImportFrequencyExtensions
    {
        public static string ShortName(this ImportFrequency value)
        {
            return value switch
            {
                ImportFrequency.Daily => "D",
                ImportFrequency.Weekly => "W",
                ImportFrequency.Monthly => "M",
                ImportFrequency.Quarterly => "Q",
                ImportFrequency.Yearly => "Y",
                ImportFrequency.OnRequest => "R",
                _ => throw new ArgumentOutOfRangeException(nameof(value))
            };
        }
    }
}
