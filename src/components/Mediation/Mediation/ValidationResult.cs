﻿using System.Collections.Generic;
using System.Linq;
using Laso.Mediation.Internals.Extensions;

namespace Laso.Mediation
{
    public class ValidationResult : Response
    {
        public ValidationResult()
        {
            IsValid = true;
        }

        public ValidationResult(IReadOnlyCollection<ValidationMessage> messages)
        {
            IsValid = !messages.Any();
            ValidationMessages.AddRange(messages);
        }

        public static ValidationResult Succeeded()
        {
            return new ValidationResult { IsValid = true };
        }

        public static ValidationResult Failed()
        {
            return new ValidationResult { IsValid = false };
        }

        public static ValidationResult Failed(string message)
        {
            return new ValidationResult { IsValid = false, ValidationMessages = { new ValidationMessage(string.Empty, message) } };
        }

        public static ValidationResult Failed(string key, string message)
        {
            return new ValidationResult { IsValid = false, ValidationMessages = { new ValidationMessage(key, message) } };
        }

        public void AddFailure(string key, string message)
        {
            AddFailure(new ValidationMessage(key, message));
        }

        public void AddFailure(ValidationMessage message)
        {
            IsValid = false;
            ValidationMessages.Add(message);
        }
    }
}