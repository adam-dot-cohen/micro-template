using Shouldly;
using Xunit;

namespace Security.KeyVaultSecrets.IntegrationTests
{
    public class TestEnvironmentTests
    {
        [Fact]
        public void Should_Succeed()
        {
            true.ShouldBeTrue();
            false.ShouldBeFalse();
        }
    }
}
