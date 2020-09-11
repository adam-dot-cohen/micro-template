using Laso.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Laso.Catalog.FunctionalTests.Hosting
{
    public class HostingTests : FunctionalTestBase<Laso.Catalog.Api.Program>
    {
        [Fact]
        public void Hosting_Should_Succeed()
        {
            Host.ShouldNotBeNull();
        }

        [Fact]
        public void TestEnvironment_ShouldBeSet()
        {
            Host.Services.GetRequiredService<IConfiguration>()["testEnvironment"].ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public void Configuration_Environment_ShouldBeSet()
        {
            new[] { "Development", "Staging" }.ShouldContain(
                Host.Services.GetRequiredService<IConfiguration>()["environment"]);
        }

        [Fact]
        public void Development_HostEnvironment_ShouldBeSet()
        {
            Host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment().ShouldBeTrue();
        }
    }
}
