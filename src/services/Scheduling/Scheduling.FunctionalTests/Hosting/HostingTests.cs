using Laso.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Laso.Scheduling.FunctionalTests.Hosting
{
    public class HostingTests : FunctionalTestBase<Laso.Scheduling.Api.Program>
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
            Host.Services.GetRequiredService<IConfiguration>()["environment"].ShouldBe("Development");
        }

        [Fact]
        public void Development_HostEnvironment_ShouldBeSet()
        {
            Host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment().ShouldBeTrue();
        }
    }
}
