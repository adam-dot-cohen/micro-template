using Laso.Testing;
using Shouldly;

namespace Laso.Insights.FunctionalTests.Services.Provisioning
{
    public class ProvisioningScenario : FunctionalTestBase<Laso.Provisioning.Api.Program>
    {
        public ProvisioningScenario()
        {
            Host.ShouldNotBeNull();
        }
    }
}
