namespace Laso.Identity.Core.Mediator
{
    public class ValidationMessage
    {
        public ValidationMessage(string key, string message)
        {
            Key = key;
            Message = message;
        }

        public string Key { get; }
        public string Message { get; }
    }
}