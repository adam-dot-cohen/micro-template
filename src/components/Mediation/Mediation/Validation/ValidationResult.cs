using System;
using System.Collections.Generic;

namespace Infrastructure.Mediation.Validation
{
    public class ValidationResult : Response
    {
        public ValidationResult()
        {
        }

        public ValidationResult(IEnumerable<ValidationMessage> failures = null, Exception exception = null) : base(failures, exception)
        {
        }

        public static ValidationResult Succeeded()
        {
            return new ValidationResult();
        }

        public static ValidationResult Failed(string message)
        {
            return new ValidationResult(new[] { new ValidationMessage(string.Empty, message) });
        }

        public static ValidationResult Failed(string key, string message)
        {
            return new ValidationResult(new[] { new ValidationMessage(key, message) });
        }

        public void AddFailure(string key, string message)
        {
            AddFailureInternal(new ValidationMessage(key, message));
        }

        public void AddFailures(params ValidationMessage[] messages)
        {
            AddFailuresInternal(messages);
        }
    }
}