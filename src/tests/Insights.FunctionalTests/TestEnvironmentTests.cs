using Shouldly;
using Xunit;

namespace Laso.Insights.FunctionalTests
{
    public class TestEnvironmentTests
    {
        [Fact]
        public void Should_Succeed()
        {
            true.ShouldBe(true);
            false.ShouldBe(false);
        }
    }
}
