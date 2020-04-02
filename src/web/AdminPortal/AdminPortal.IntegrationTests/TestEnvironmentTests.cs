using Shouldly;
using Xunit;

namespace Laso.AdminPortal.IntegrationTests
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
