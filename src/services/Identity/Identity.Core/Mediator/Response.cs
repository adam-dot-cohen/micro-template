using System;
using System.Collections.Generic;
using System.Linq;
using Laso.Identity.Core.Extensions;

namespace Laso.Identity.Core.Mediator
{
    public class Response
    {
        public bool IsValid { get; set; }
        public IList<ValidationMessage> ValidationMessages { get; set; } = new List<ValidationMessage>();
        public Exception Exception { get; set; }

        public bool Success()
        {
            return IsValid && Exception == null;
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
                IsValid = IsValid,
                ValidationMessages = new List<ValidationMessage>(ValidationMessages),
                Exception = Exception
            };
        }
    }

    public class Response<TResult> : Response
    {
        public TResult Result { get; set; }
    }
}