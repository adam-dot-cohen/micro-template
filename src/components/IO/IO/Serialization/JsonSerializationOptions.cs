namespace Laso.IO.Serialization
{
    public class JsonSerializationOptions
    {
        public bool IncludeNulls { get; set; }
        public CasingStyle PropertyNameCasingStyle { get; set; }
    }

    public enum CasingStyle
    {
        Pascal,
        Camel
    }
}