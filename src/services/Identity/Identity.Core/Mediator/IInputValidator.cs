using System.Collections.Generic;
using System.Linq;
using Laso.Identity.Core.Extensions;

namespace Laso.Identity.Core.Mediator
{
    // Conventions:
    //  - ONE per IRequest
    //  - MUST be implemented on the Message
    public interface IInputValidator
    {
        ValidationResult ValidateInput();
    }

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