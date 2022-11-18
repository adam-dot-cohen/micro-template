using System.Net;
using System.Threading.Tasks;
using Laso.Testing;
using Shouldly;
using Xunit;

namespace Laso.Subscription.FunctionalTests.Health
{
    public class HealthCheckTests : FunctionalTestBase<Laso.Subscription.Api.Program>
    {
        [Fact]
        public async Task HealthCheck_Should_Succeed()
        {
            var response = await Client.GetAsync("/health");

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
            response.Content.Headers.ContentType!.MediaType.ShouldBe("application/json");
        }
    }
}
