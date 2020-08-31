using System;
using Insights.AccountTransactionClassifier.Function;
using Insights.AccountTransactionClassifier.Function.Classifier;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace AccountTransactionClassifier.FunctionalTests
{
    [Trait("Capability", "Managed Identity")]   // NOTE: Currently, this is required via configuration.
    public class StartupTests
    {
        [Fact]
        public void Should_Startup()
        {
            var builder = new FunctionsHostBuilder();

            var startup = new Startup();
            startup.Configure(builder);

        }

        [Fact]
        public void Should_Build_AccountTransactionClassifier()
        {
            var builder = new FunctionsHostBuilder();

            var startup = new Startup();
            startup.Configure(builder);

            var services = builder.Build();

            var classifier = services.GetRequiredService<IAccountTransactionClassifier>();
            classifier.ShouldNotBeNull();
        }
    }

    public class FunctionsHostBuilder : IFunctionsHostBuilder
    {
        public IServiceCollection Services { get; } = new ServiceCollection();

        public IServiceProvider Build()
        {
            return Services.BuildServiceProvider();
        }
    }
}
