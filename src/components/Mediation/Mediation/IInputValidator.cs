namespace Laso.Mediation
{
    // Conventions:
    //  - ONE per IRequest
    //  - MUST be implemented on the Message
    public interface IInputValidator
    {
        ValidationResult ValidateInput();
    }
}