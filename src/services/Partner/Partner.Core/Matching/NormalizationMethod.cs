using System;
using Partner.Core.Extensions;

namespace Partner.Core.Matching
{
    public static class NormalizationMethod
    {
        public static T TaxId<T>(T input)
        {
            var val = input as string ?? Convert.ToString(input);

            return val
                .IfNotNullOrEmpty(s => s.Clean(Normalize.DigitsOnly))
                .ConvertTo<T>();
        }

        public static T Zip5<T>(T input)
        {
            var val = input as string ?? Convert.ToString(input);

            return val
                .IfNotNullOrEmpty(s => s.Clean(Normalize.DigitsOnly).Truncate(5))
                .ConvertTo<T>();
        }

        public static T Phone10<T>(T input)
        {
            var val = input as string ?? Convert.ToString(input);

            return val
                .IfNotNullOrEmpty(s => s.Clean(Normalize.DigitsOnly).Right(10))
                .ConvertTo<T>();
        }
    }
}
