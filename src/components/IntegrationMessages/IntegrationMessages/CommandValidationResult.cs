using System.Collections.Generic;

namespace Laso.IntegrationMessages
{
    public class CommandValidationResult
    {
        private List<InValidPropertyState> _inValidProperties => new List<InValidPropertyState>();
        public bool IsValid { get; set; }
        public ICollection<InValidPropertyState> ValidationIssues => _inValidProperties;

        public void AddFailure(string propertyName, string message)
        {
            _inValidProperties.Add(new InValidPropertyState {Property = propertyName, Message = message});
            IsValid = false;
        }

        public static CommandValidationResult Valid()
        {
            return new CommandValidationResult {IsValid = true};
        }
    }
}