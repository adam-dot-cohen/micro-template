using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Laso.Provisioning.IntegrationTests.Hosting
{
    public class HostingStartupTests : IntegrationTestBase
    {
        public class When_Hosted_As_Development : HostingStartupTests
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

            [Fact]
            public void DependencyResolution_Should_Be_Configured()
            {
                Services.GetService<IServiceProvider>().ShouldNotBeNull();
            }

            [Fact]
            public void Host_Should_Be_Test()
            {
                Services.GetRequiredService<IConfiguration>()["testEnvironment"].ShouldNotBeNullOrEmpty();
            }

            [Fact]
            public void Environment_Should_Be_Development()
            {
                Services.GetRequiredService<IHostEnvironment>().EnvironmentName.ShouldBe("Development");
                Services.GetRequiredService<IHostEnvironment>().IsDevelopment().ShouldBeTrue();
            }
        }
    }
}
