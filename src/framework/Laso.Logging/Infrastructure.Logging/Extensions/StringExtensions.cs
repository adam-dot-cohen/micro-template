namespace Infrastructure.Logging.Extensions
{
    public static class StringExtensions
    {

        public static string EmptyStringToNull(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }


    }
}
