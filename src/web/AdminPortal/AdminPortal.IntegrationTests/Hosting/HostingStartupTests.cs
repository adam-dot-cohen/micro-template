using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Laso.AdminPortal.IntegrationTests.Hosting
{
    public abstract class HostingStartupTests : IntegrationTestBase
    {
        public class When_Hosted : HostingStartupTests
        {
            [Fact]
            public void Services_Should_Be_Configured()
            {
                Services.ShouldNotBeNull();
            }

            [Fact]
            public void Configuration_Should_Be_Configured()
            {
                Configuration.ShouldNotBeNull();
                Configuration.AsEnumerable().ShouldNotBeEmpty();
            }
        }
    }
}
