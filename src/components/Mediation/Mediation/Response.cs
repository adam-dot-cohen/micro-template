using System;
using System.Collections.Generic;
using System.Linq;
using Laso.Mediation.Internals.Extensions;

namespace Laso.Mediation
{
    /// <summary>
    /// Common response indicating success or errors.
    /// Attempts to be immutable-ish while leaving room for derived classes to mutate state if needed.
    /// </summary>
    public abstract class Response
    {
        protected Response(IEnumerable<ValidationMessage> failures = null, Exception exception = null)
        {
            _validationMessages = failures?.ToList() ?? new List<ValidationMessage>();
            Exception = exception;
        }

        public bool Success => ValidationMessages.Count == 0 && Exception == null;

        private readonly List<ValidationMessage> _validationMessages;
        public IReadOnlyList<ValidationMessage> ValidationMessages => _validationMessages;

        protected void AddFailureInternal(ValidationMessage failure)
        {
            _validationMessages.Add(failure);
        }

        protected void AddFailuresInternal(IEnumerable<ValidationMessage> failures)
        {
            _validationMessages.AddRange(failures);
        }

        public Exception Exception { get; protected internal set; }

        public string GetAllMessages()
        {
            return new[] { Exception?.InnermostException()?.Message }
                .Concat(ValidationMessages.Select(x => x.Message))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Join("; ");
        }

        public TResponse ToResponse<TResponse>() where TResponse : Response, new()
        {
            var response = new TResponse { Exception = Exception };
            response.AddFailuresInternal(ValidationMessages);

            return response;
        }

        protected static TResponse From<TResponse>(ICollection<Response> responses) where TResponse : Response, new()
        {
            var response = new TResponse { Exception = responses.FirstOrDefault(x => x.Exception != null)?.Exception };
            response.AddFailuresInternal(responses.SelectMany(x => x.ValidationMessages));

            return response;
        }
    }
}
