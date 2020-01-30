using System;
using Laso.Logging.Configuration;
using Laso.Logging.UnitTests.Extensions;
using Xunit;

namespace Laso.Logging.UnitTests.ConfigurationTests
{
    public class LoggingConfigurationBuilderTests
    {
        [Fact]
        public void should_build_basic_values()
        {
            var configuration = new LoggingConfigurationBuilder().Build();

            var levels = (LogLevel[]) Enum.GetValues(typeof(LogLevel));

            foreach (var level in levels)
            {
                configuration.LogLevels[level].ShouldBeTrue();
            }

            configuration.ShutDownActions.Count.ShouldEqual(1);
        }

    }

}
