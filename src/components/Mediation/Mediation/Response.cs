using System;
using System.Collections.Generic;
using System.Linq;
using Laso.Mediation.Internals.Extensions;

namespace Laso.Mediation
{
    public abstract class Response
    {
        // TODO: Does this get serialized?
        // Property for serialization purposes
        public bool IsSuccess => Success();

        public IList<ValidationMessage> ValidationMessages { get; set; } = new List<ValidationMessage>();

        public Exception Exception { get; set; }

        public bool Success()
        {
            return ValidationMessages.Count == 0 && Exception == null;
        }

        public string GetAllMessages()
        {
            return new[] { Exception?.InnermostException()?.Message }
                .Concat(ValidationMessages.Select(x => x.Message))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Join("; ");
        }

        public TResponse ToResponse<TResponse>() where TResponse : Response, new()
        {
            return new TResponse
            {
                ValidationMessages = new List<ValidationMessage>(ValidationMessages),
                Exception = Exception
            };
        }
    }
}
