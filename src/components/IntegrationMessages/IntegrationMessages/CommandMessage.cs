namespace Laso.IntegrationMessages
{
    public abstract class CommandMessage : IIntegrationMessage
    {
        public abstract CommandValidationResult ValidateInput(IIntegrationMessage command);

        public CommandValidationResult NoValidationNeeded()
        {
            return CommandValidationResult.Valid();
        }
        //TODO: add helpers for invalidating based on null or default
        //TODO: add property attribute based validation
    }
}