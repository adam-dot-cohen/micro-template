using System;

namespace DataImport.Services.DTOs
{
    public enum ImportFrequencyDto
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
        public static string ShortName(this ImportFrequencyDto value)
        {
            return value switch
            {
                ImportFrequencyDto.Daily => "D",
                ImportFrequencyDto.Weekly => "W",
                ImportFrequencyDto.Monthly => "M",
                ImportFrequencyDto.Quarterly => "Q",
                ImportFrequencyDto.Yearly => "Y",
                ImportFrequencyDto.OnRequest => "R",
                _ => throw new ArgumentOutOfRangeException(nameof(value))
            };
        }
    }
}
