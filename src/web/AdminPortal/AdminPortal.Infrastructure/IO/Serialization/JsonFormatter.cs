using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Laso.AdminPortal.Infrastructure.IO.Serialization
{
    //more here: https://stackoverflow.com/questions/4580397/json-formatter-in-c
    public static class JsonFormatter
    {
        public static string Prettify(string json)
        {
            var jObj = JObject.Parse(json);

            return jObj.ToString(Formatting.Indented);
        }
    }
}
