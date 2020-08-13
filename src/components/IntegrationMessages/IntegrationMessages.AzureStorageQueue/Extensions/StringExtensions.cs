namespace Laso.IntegrationMessages.AzureStorageQueue.Extensions
{
    internal static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (value != null && value.Length > maxLength)
                value = value.Substring(0, maxLength);

            return value;
        }
    }
}
