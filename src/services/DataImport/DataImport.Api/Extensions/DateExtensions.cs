using System;

namespace Laso.DataImport.Api.Extensions
{
    public static class DateExtensions
    {
        public static DateTime ToDateTime(this Date date)
        {
            if (date == null)
                throw new ArgumentNullException(nameof(date));

            return new DateTime(date.Year, date.Month, date.Day);
        }
    }
}
