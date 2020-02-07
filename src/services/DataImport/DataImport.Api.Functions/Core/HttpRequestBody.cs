using System.Collections.Generic;

namespace DataImport.Api.Core
{
    internal class HttpRequestBody<T>
    {
        public bool IsValid { get; set; }
        public T Model { get; set; }
        public IEnumerable<string> ValidationMessages { get; set; }
    }
}
