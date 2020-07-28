namespace Laso.Mediation.UnitTests
{
    public class TestQuery : IQuery<TestResult>, IInputValidator
    {
        private readonly ValidationMessage[] _messages;

        public TestQuery(params ValidationMessage[] messages)
        {
            _messages = messages;
        }

        public ValidationResult ValidateInput()
        {
            var result = new ValidationResult();
            result.AddFailures(_messages);

            return result;
        }
    }

    public class TestResult
    {

    }
}