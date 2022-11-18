using System;
using Infrastructure.Logging.Configuration;
using Infrastructure.Logging.UnitTests.Extensions;
using Xunit;

namespace Infrastructure.Logging.UnitTests.ConfigurationTests
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
        
        [Fact]
        public void should_disable_all_levels()
        {
            var configuration = new LoggingConfigurationBuilder()
                .WithLoggingLevel(LogLevel.Debug,false)
                .WithLoggingLevel(LogLevel.Information,false)
                .WithLoggingLevel(LogLevel.Warning,false)
                .WithLoggingLevel(LogLevel.Error,false)
                .WithLoggingLevel(LogLevel.Exception,false)
                .Build();
         
            var levels = (LogLevel[]) Enum.GetValues(typeof(LogLevel));

            foreach (var level in levels)
            {
                configuration.LogLevels[level].ShouldBeFalse();
            }
        }
    }
}
