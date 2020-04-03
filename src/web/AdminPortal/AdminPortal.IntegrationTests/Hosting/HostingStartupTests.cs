using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Laso.AdminPortal.IntegrationTests.Hosting
{
    public abstract class HostingStartupTests : IntegrationTestBase
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
                Services.GetService<IServiceProvider>().ShouldBeOfType<Lamar.Container>();
            }

            [Fact]
            public void Environment_Should_Be_Development()
            {
                Services.GetRequiredService<IHostEnvironment>().EnvironmentName.ShouldBe("Development");
                Services.GetRequiredService<IHostEnvironment>().IsDevelopment().ShouldBeTrue();
            }
        }

        public class When_Hosted_As_Production : HostingStartupTests
        {
            public When_Hosted_As_Production()
            {
                // TODO: Maybe some helpers in the base to set specific configuration?
                ConfigureTestConfiguration(builder =>
                    builder.AddInMemoryCollection(new[] { new KeyValuePair<string, string>("environment", "Production") }));
            }

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
                Services.GetService<IServiceProvider>().ShouldBeOfType<Lamar.Container>();
            }

            [Fact]
            public void Environment_Should_Be_Production()
            {
                Services.GetRequiredService<IHostEnvironment>().EnvironmentName.ShouldBe("Production");
                Services.GetRequiredService<IHostEnvironment>().IsProduction().ShouldBeTrue();
            }
        }
    }
}
