using Laso.Provisioning.FunctionalTests;
using Shouldly;

namespace Laso.Insights.FunctionalTests.Services.Provisioning
{
    public class ProvisioningScenario : FunctionalTestBase
    {
        public ProvisioningScenario()
        {
            Host.ShouldNotBeNull();
        }
    }
}
