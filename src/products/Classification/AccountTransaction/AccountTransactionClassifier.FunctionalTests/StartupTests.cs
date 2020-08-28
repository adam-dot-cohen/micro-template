using Insights.AccountTransactionClassifier.Function;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace AccountTransactionClassifier.FunctionalTests
{
    [Trait("Capability", "Managed Identity")]   // NOTE: Currently, this is required via configuration.
    public class StartupTests
    {
        [Fact]
        public void Should_Startup()
        {
            var builder = Substitute.For<IFunctionsHostBuilder>();

            var startup = new Startup();
            startup.Configure(builder);

        }
    }
}
