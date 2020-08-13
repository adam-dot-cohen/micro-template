using System;
using System.Collections.Generic;

namespace Laso.Mediation
{
    public class EventResponse : Response
    {
        public EventResponse() { }

        public EventResponse(IEnumerable<ValidationMessage> failures = null, Exception exception = null) : base(failures, exception)
        {
        }

        public static EventResponse Succeeded() => new EventResponse();

        public static EventResponse Failed(string message) => new EventResponse(new[] { new ValidationMessage(string.Empty, message) });
        public static EventResponse Failed(string key, string message) => new EventResponse(new[] { new ValidationMessage(key, message) });
        public static EventResponse Failed(ValidationMessage message) => new EventResponse(new[] { message });
        public static EventResponse Failed(IEnumerable<ValidationMessage> messages) => new EventResponse(messages);
        public static EventResponse Failed(Exception exception) => new EventResponse(exception: exception);
        public static EventResponse Failed(Response response)
        {
            if (response.Success)
            {
                throw new Exception($"Expected failure response of type: {response.GetType().Name}");
            }

            return response.ToResponse<EventResponse>();
        }
    }
}