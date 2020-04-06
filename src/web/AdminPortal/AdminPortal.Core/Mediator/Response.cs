using System;
using System.Collections.Generic;
using System.Linq;
using Laso.AdminPortal.Core.Extensions;

namespace Laso.AdminPortal.Core.Mediator
{
    public abstract class Response
    {
        public bool IsValid { get; set; }
        public List<ValidationMessage> ValidationMessages { get; set; } = new List<ValidationMessage>();
        public Exception Exception { get; set; }
        public bool Success => IsValid && (Exception == null);

        public string GetAllMessages()
        {
            return new[] { Exception?.InnermostException()?.Message }
                .Concat(ValidationMessages.Select(x => x.Message))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Join(", ");
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
}
